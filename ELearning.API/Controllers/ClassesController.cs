using System.Security.Claims;
using ELearning.Core.DTOs.Class;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassesController(IClassService classService) : ControllerBase
{
    private readonly IClassService _classService = classService;

    // 🛡️ Security Method: Kiểm tra xem User hiện tại có phải chủ lớp hoặc Admin không
    private async Task<bool> IsClassOwnerOrAdmin(Guid classId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out Guid userId)) return false;

        return await _classService.IsClassOwnerOrAdminAsync(classId, userId, role ?? "");
    }

    // 🛡️ BẢO MẬT: Ẩn các lớp của GV khác
    [HttpGet]
    [EndpointSummary("Lấy toàn bộ danh sách lớp")]
    [EndpointDescription("Truy xuất danh sách tất cả lớp có sẵn trong hệ thống.")]
    public async Task<IActionResult> GetAll()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        Guid? instructorId = null;

        if (role == "Instructor")
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                instructorId = userId;
            }
        }

        var classes = await _classService.GetAllClassesAsync(instructorId);
        return Ok(classes);
    }

    [HttpPost]
    [EndpointSummary("Tạo mới lớp học")]
    [EndpointDescription("Tạo một lớp học mới trong hệ thống.")]
    public async Task<IActionResult> Create([FromBody] CreateClassRequestDto request)
    {
        try
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Instructor")
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdStr, out Guid userId))
                {
                    // Frontend của Giảng viên không gửi lên InstructorId, 
                    // nên ta tự động gán luôn ID của họ vào để pass qua bảo mật
                    request = request with { InstructorId = userId };
                }
            }

            return Ok(await _classService.CreateClassAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/enroll")]
    [EndpointSummary("Ghi danh khóa học")]
    [EndpointDescription("Thêm học viên vào một lớp học/khóa học.")]
    public async Task<IActionResult> EnrollStudent(Guid id, [FromBody] EnrollStudentRequestDto request)
    {
        if (!await IsClassOwnerOrAdmin(id)) return Forbid();

        var success = await _classService.EnrollStudentAsync(id, request.StudentId);
        if (!success) return BadRequest(new { message = "Lỗi khi ghi danh sinh viên." });
        return Ok(new { message = "Ghi danh thành công!" });
    }

    [HttpPost("{id:guid}/enroll-by-email")]
    [EndpointSummary("Ghi danh qua Email hoặc Mã")]
    [EndpointDescription("Thêm học viên vào một lớp học dựa trên Email hoặc Mã Sinh Viên.")]
    public async Task<IActionResult> EnrollStudentByEmail(Guid id, [FromBody] EnrollStudentByEmailRequestDto request)
    {
        if (!await IsClassOwnerOrAdmin(id)) return Forbid();

        try
        {
            var success = await _classService.EnrollStudentByEmailAsync(id, request.EmailOrCode);
            if (!success) return BadRequest(new { message = "Lỗi khi ghi danh sinh viên." });
            return Ok(new { message = "Ghi danh thành công!" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Cập nhật lớp học")]
    [EndpointDescription("Cập nhật thông tin của một lớp học đã tồn tại.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequestDto request)
    {
        try
        {
            if (!await IsClassOwnerOrAdmin(id)) return Forbid();

            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Instructor")
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdStr, out Guid userId))
                {
                    request = request with { InstructorId = userId };
                }
            }

            var isUpdated = await _classService.UpdateClassAsync(id, request);
            if (!isUpdated) return NotFound(new { message = "Không tìm thấy lớp học để cập nhật" });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Xóa lớp học")]
    [EndpointDescription("Xóa vĩnh viễn hoặc khóa một lớp học khỏi hệ thống.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await IsClassOwnerOrAdmin(id)) return Forbid();

        var isDeleted = await _classService.DeleteClassAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy lớp học để xóa" });
        return NoContent();
    }

    // API lấy chi tiết Lớp và danh sách Sinh viên trong lớp đó
    [HttpGet("{id:guid}/details")]
    [EndpointSummary("Lấy chi tiết lớp học")]
    [EndpointDescription("Truy xuất thông tin chi tiết của một lớp học và danh sách sinh viên trong lớp.")]
    public async Task<IActionResult> GetClassDetails(Guid id)
    {
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? "";

        if (!Guid.TryParse(currentUserIdStr, out Guid currentUserId)) return Unauthorized();

        try
        {
            var details = await _classService.GetClassDetailsAsync(id, currentUserId, currentUserRole);
            if (details == null) return NotFound("Không tìm thấy lớp học");
            return Ok(details);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // Lấy danh sách Lớp học mà một Sinh viên đang tham gia
    [HttpGet("student/{studentId:guid}")]
    [EndpointSummary("Lấy chi tiết lớp học")]
    [EndpointDescription("Truy xuất thông tin chi tiết của một lớp học và danh sách sinh viên trong lớp.")]
    public async Task<IActionResult> GetClassesByStudent(Guid studentId)
    {
        var classes = await _classService.GetClassesByStudentAsync(studentId);
        return Ok(classes);
    }

    // API IMPORT EXCEL CHO LỚP HỌC
    [HttpPost("{id:guid}/import-students")]
    [EndpointSummary("Nhập danh sách sinh viên từ file Excel")]
    [EndpointDescription("Xử lý file Excel được tải lên và import danh sách sinh viên vào lớp học.")]
    public async Task<IActionResult> ImportStudentsToClass(Guid id, IFormFile file)
    {
        if (!await IsClassOwnerOrAdmin(id)) return Forbid();
        if (file == null || file.Length == 0) return BadRequest("File Excel trống!");

        var result = await _classService.ImportStudentsFromExcelAsync(id, file);

        return Ok(new
        {
            message = $"Đã thêm thành công {result.AddedCount} sinh viên vào lớp.",
            errors = result.Errors
        });
    }

    //  Xóa Sinh viên khỏi lớp (Đuổi học)
    [HttpDelete("{classId:guid}/remove-student/{studentId:guid}")]
    [EndpointSummary("Xóa sinh viên khỏi lớp")]
    [EndpointDescription("Xóa vĩnh viễn hoặc khóa một sinh viên khỏi lớp.")]
    public async Task<IActionResult> RemoveStudentFromClass(Guid classId, Guid studentId)
    {
        if (!await IsClassOwnerOrAdmin(classId)) return Forbid();

        var success = await _classService.RemoveStudentFromClassAsync(classId, studentId);
        if (!success) return NotFound("Sinh viên không tồn tại trong lớp này.");

        return NoContent();
    }
}