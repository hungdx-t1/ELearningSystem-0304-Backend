using ELearning.Core.DTOs.Course;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController(ICourseService courseService, AppDbContext context) : ControllerBase
{
    private readonly ICourseService _courseService = courseService;
    private readonly AppDbContext _context = context;

    private async Task<bool> IsCourseCreatorOrAdmin(Guid courseId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin") return true;

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out Guid userId)) return false;

        var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
        return course != null && course.CreatorId == userId;
    }

    [HttpGet]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy danh sách khóa học")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất danh sách tất cả các khóa học có sẵn trong hệ thống.")]
    public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetAll()
    {
        var courses = await _courseService.GetAllCoursesAsync();

        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Instructor")
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                // Chỉ lấy khóa học do chính giảng viên này tạo
                courses = courses.Where(c => c.CreatorId == userId).ToList();
            }
        }
        return Ok(courses);
    }

    [HttpGet("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy chi tiết khóa học bằng ID")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất thông tin chi tiết của một khóa học cụ thể thông qua ID.")]
    public async Task<ActionResult<CourseResponseDto>> GetById(Guid id, [FromServices] AppDbContext context)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound(new { message = "Không tìm thấy khóa học" });

        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

        if (Guid.TryParse(currentUserIdStr, out Guid currentUserId))
        {
            if (currentUserRole == "Student")
            {
                var isEnrolledInCourse = await context.ClassEnrollments
                    .AnyAsync(e => e.StudentId == currentUserId && e.Class.CourseId == id);
                if (!isEnrolledInCourse) return Forbid();
            }
            else if (currentUserRole == "Instructor")
            {
                // Giảng viên được xem nếu khóa học là Public HOẶC là khóa do mình tạo
                if (!course.IsPublic && course.CreatorId != currentUserId)
                    return Forbid();
            }
        }
        return Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = "Instructor, Admin")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Tạo mới khóa học")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Tạo một khóa học mới trong hệ thống.")]
    public async Task<ActionResult<CourseResponseDto>> Create([FromBody] CreateCourseRequestDto request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out Guid creatorId))
        {
            return Unauthorized(new { message = "Không xác định được ID người dùng hợp lệ." });
        }

        // Truyền ID người tạo xuống Service
        var newCourse = await _courseService.CreateCourseAsync(request, creatorId);
        return CreatedAtAction(nameof(GetById), new { id = newCourse.Id }, newCourse);
    }

    [HttpPost("{id:guid}/copy")]
    [Authorize(Roles = "Instructor, Admin")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Nhân bản khóa học")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Nhân bản một khóa học đã tồn tại.")]
    public async Task<IActionResult> Copy(Guid id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out Guid currentUserId))
        {
            return Unauthorized(new { message = "Không xác định được ID người dùng hợp lệ." });
        }

        var copiedCourse = await _courseService.CopyCourseAsync(id, currentUserId);

        if (copiedCourse == null)
            return BadRequest(new { message = "Khóa học không tồn tại hoặc chủ sở hữu chưa cho phép sao chép (Chưa Public)." });

        return Ok(new { message = "Nhân bản khóa học thành công!", data = copiedCourse });
    }

    [HttpPut("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Cập nhật khóa học")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Cập nhật thông tin của một khóa học đã tồn tại.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequestDto request)
    {
        if (!await IsCourseCreatorOrAdmin(id)) return Forbid();

        var isUpdated = await _courseService.UpdateCourseAsync(id, request);
        if (!isUpdated) return NotFound(new { message = "Không tìm thấy khóa học để cập nhật" });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Xóa khóa học")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Xóa vĩnh viễn hoặc khóa một khóa học khỏi hệ thống.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await IsCourseCreatorOrAdmin(id)) return Forbid();

        var isDeleted = await _courseService.DeleteCourseAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy khóa học để xóa" });
        return NoContent();
    }

    [HttpGet("{id:guid}/assignments")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy danh sách bài tập của khóa học")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất danh sách các bài tập tự luận của một khóa học.")]
    public async Task<IActionResult> GetAssignmentsByCourse(Guid id)
    {
        if (!await IsCourseCreatorOrAdmin(id)) return Forbid();

        var assignments = await _courseService.GetAssignmentsByCourseAsync(id);

        if (assignments == null || !assignments.Any())
            return NotFound(new { message = "Khóa học này chưa có bài tập tự luận nào." });

        return Ok(assignments);
    }
}