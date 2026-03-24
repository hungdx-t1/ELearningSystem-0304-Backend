using ELearning.Core.DTOs.Class;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IClassService _classService;

    public ClassesController(IClassService classService, AppDbContext context)
    {
        _classService = classService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _classService.GetAllClassesAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequestDto request)
    {
        return Ok(await _classService.CreateClassAsync(request));
    }

    [HttpPost("{id:guid}/enroll")]
    public async Task<IActionResult> EnrollStudent(Guid id, [FromBody] EnrollStudentRequestDto request)
    {
        var success = await _classService.EnrollStudentAsync(id, request.StudentId);
        if (!success) return BadRequest("Lỗi khi ghi danh sinh viên.");
        return Ok(new { message = "Ghi danh thành công!" });
    }

    // PUT: api/classes/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassRequestDto request)
    {
        var isUpdated = await _classService.UpdateClassAsync(id, request);
        if (!isUpdated) return NotFound(new { message = "Không tìm thấy lớp học để cập nhật" });
        return NoContent(); // Code 204: Cập nhật thành công
    }

    // DELETE: api/classes/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _classService.DeleteClassAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy lớp học để xóa" });
        return NoContent(); // Code 204: Xóa thành công
    }

    // API lấy chi tiết Lớp và danh sách Sinh viên trong lớp đó
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetClassDetails(Guid id, [FromServices] AppDbContext context)
    {
        var classEntity = await context.Classes
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (classEntity == null) return NotFound("Không tìm thấy lớp học");

        // Lấy danh sách SV đã ghi danh
        var students = await context.ClassEnrollments
            .Include(e => e.Student)
            .Where(e => e.ClassId == id)
            .Select(e => new
            {
                Id = e.Student.Id, // Nhớ gửi Id để Angular còn biết đường Xóa (Đuổi học)
                FullName = e.Student.FullName,
                Email = e.Student.Email,
                StudentCode = e.Student.UserCode ?? "Chưa cấp mã",
                JoinDate = e.EnrollmentDate.ToString("dd/MM/yyyy HH:mm")
            })
            .ToListAsync();

        return Ok(new
        {
            Id = classEntity.Id,
            ClassCode = classEntity.ClassCode,
            ClassName = classEntity.ClassName,
            CourseName = classEntity.Course?.Title ?? "Không rõ khóa học",
            GoogleMeetLink = classEntity.GoogleMeetLink,
            AcademicYear = classEntity.AcademicYear,
            Students = students
        });
    }

    // Lấy danh sách Lớp học mà một Sinh viên đang tham gia
    [HttpGet("student/{studentId:guid}")]
    public async Task<IActionResult> GetClassesByStudent(Guid studentId, [FromServices] AppDbContext context)
    {
        var classes = await context.ClassEnrollments
            .Include(e => e.Class)
            .ThenInclude(c => c.Course) // Kéo theo thông tin Khóa học (Môn học)
            .Where(e => e.StudentId == studentId)
            .Select(e => new 
            {
                Id = e.Class.Id,
                CourseId = e.Class.CourseId,
                ClassCode = e.Class.ClassCode,
                ClassName = e.Class.ClassName,
                CourseName = e.Class.Course != null ? e.Class.Course.Title : "Chưa rõ môn",
                Schedule = e.Class.AcademicYear,
                GoogleMeetLink = e.Class.GoogleMeetLink
            })
            .ToListAsync();

        return Ok(classes);
    }

    // API IMPORT EXCEL CHO LỚP HỌC
    [HttpPost("{id:guid}/import-students")]
    public async Task<IActionResult> ImportStudentsToClass(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File Excel trống!");

        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");
        var addedCount = 0;
        var errorRows = new List<string>();

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Bỏ dòng tiêu đề
                {
                    // Cột 1: Mã SV, Cột 2: Email (Bạn có thể quy định với Giảng viên như vậy)
                    var code = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(email)) continue;

                    // Cố gắng tìm Sinh viên trong DB
                    var student = await _context.Users.FirstOrDefaultAsync(u => 
                        u.UserCode == code || u.Email == email);

                    if (student == null)
                    {
                        errorRows.Add($"Dòng {row}: Không tìm thấy Sinh viên '{code ?? email}' trong hệ thống.");
                        continue;
                    }

                    // Check xem nó có nằm trong lớp này chưa
                    var isEnrolled = await _context.ClassEnrollments.AnyAsync(e => e.ClassId == id && e.StudentId == student.Id);
                    if (!isEnrolled)
                    {
                        await _classService.EnrollStudentAsync(id, student.Id);
                        addedCount++;
                    }
                }
            }
        }

        return Ok(new { 
            message = $"Đã thêm thành công {addedCount} sinh viên vào lớp.", 
            errors = errorRows 
        });
    }
    
    //  Xóa Sinh viên khỏi lớp (Đuổi học)
    [HttpDelete("{classId:guid}/remove-student/{studentId:guid}")]
    public async Task<IActionResult> RemoveStudentFromClass(Guid classId, Guid studentId, [FromServices] AppDbContext context)
    {
        var enrollment = await _context.ClassEnrollments
            .FirstOrDefaultAsync(e => e.ClassId == classId && e.StudentId == studentId);
            
        if (enrollment == null) return NotFound("Sinh viên không tồn tại trong lớp này.");

        _context.ClassEnrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        
        return NoContent(); // Code 204
    }
}