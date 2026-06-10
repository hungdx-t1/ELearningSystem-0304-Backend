using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Core.DTOs.Class;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace ELearning.Services.Implements;

public class ClassService(IGenericRepository<Class> classRepo, IGenericRepository<ClassEnrollment> enrollmentRepo, AppDbContext context) : IClassService
{
    public async Task<bool> IsClassOwnerOrAdminAsync(Guid classId, Guid userId, string role)
    {
        if (role == "Admin") return true;
        var classEntity = await context.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == classId);
        return classEntity != null && classEntity.InstructorId == userId;
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync(Guid? instructorId = null)
    {
        var query = context.Classes.AsQueryable();
        if (instructorId.HasValue)
        {
            query = query.Where(c => c.InstructorId == instructorId.Value);
        }

        return await query
            .Select(c => new ClassResponseDto(
                c.Id,
                c.CourseId,
                c.ClassCode,
                c.ClassName,
                c.InstructorId ?? Guid.Empty, // Tránh lỗi null nếu GV đã nghỉ việc
                c.GoogleMeetLink,
                c.AcademicYear,
                c.Description,
                c.Enrollments.Count
            ))
            .ToListAsync();
    }

    public async Task<ClassResponseDto> CreateClassAsync(CreateClassRequestDto request)
    {
        var existingCode = await classRepo.FindAsync(c => c.ClassCode.ToLower() == request.ClassCode.ToLower());
        if (existingCode.Any())
        {
            throw new InvalidOperationException("Mã lớp học đã tồn tại. Vui lòng nhập mã khác.");
        }

        var newClass = new Class
        {
            Id = Guid.NewGuid(),
            CourseId = request.CourseId,         // Hứng CourseId từ Angular
            ClassCode = request.ClassCode,
            ClassName = request.ClassName,
            InstructorId = request.InstructorId, // Hứng Giảng viên được phân công từ Angular
            GoogleMeetLink = request.GoogleMeetLink,
            AcademicYear = request.AcademicYear,
            Description = request.Description
        };

        await classRepo.AddAsync(newClass);
        await classRepo.SaveChangesAsync();

        return new ClassResponseDto(
            newClass.Id,
            newClass.CourseId,
            newClass.ClassCode,
            newClass.ClassName,
            newClass.InstructorId ?? Guid.Empty,
            newClass.GoogleMeetLink,
            newClass.AcademicYear,
            newClass.Description,
            0
        );
    }

    public async Task<bool> EnrollStudentAsync(Guid classId, Guid studentId)
    {
        // Kiểm tra xem đã ghi danh chưa để tránh lỗi Duplicate
        var exists = await enrollmentRepo.FindAsync(e => e.ClassId == classId && e.StudentId == studentId);
        if (exists.Any()) return true; // Đã vào lớp rồi

        var enrollment = new ClassEnrollment
        {
            ClassId = classId,
            StudentId = studentId,
            EnrollmentDate = DateTime.UtcNow
        };
        await enrollmentRepo.AddAsync(enrollment);
        return await enrollmentRepo.SaveChangesAsync();
    }

    public async Task<bool> EnrollStudentByEmailAsync(Guid classId, string emailOrCode)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == emailOrCode || u.UserCode == emailOrCode || u.FullName.Contains(emailOrCode));
        if (user == null) throw new KeyNotFoundException("Không tìm thấy Sinh viên này trong hệ thống!");

        return await EnrollStudentAsync(classId, user.Id);
    }

    public async Task<bool> UpdateClassAsync(Guid id, UpdateClassRequestDto request)
    {
        var existingClass = await classRepo.GetByIdAsync(id);
        if (existingClass == null) return false; // Không tìm thấy lớp

        var duplicateCode = await classRepo.FindAsync(c => c.Id != id && c.ClassCode.ToLower() == request.ClassCode.ToLower());
        if (duplicateCode.Any())
        {
            throw new InvalidOperationException("Mã lớp học đã tồn tại. Vui lòng nhập mã khác.");
        }

        // Cập nhật các trường thông tin
        existingClass.CourseId = request.CourseId;
        existingClass.ClassCode = request.ClassCode;
        existingClass.ClassName = request.ClassName;
        existingClass.InstructorId = request.InstructorId;
        existingClass.GoogleMeetLink = request.GoogleMeetLink;
        existingClass.AcademicYear = request.AcademicYear;
        existingClass.Description = request.Description;

        classRepo.Update(existingClass);
        return await classRepo.SaveChangesAsync();
    }

    public async Task<bool> DeleteClassAsync(Guid id)
    {
        var existingClass = await classRepo.GetByIdAsync(id);
        if (existingClass == null) return false;

        classRepo.Delete(existingClass);
        return await classRepo.SaveChangesAsync();
    }

    public async Task<ClassDetailsResponseDto?> GetClassDetailsAsync(Guid classId, Guid currentUserId, string currentUserRole)
    {
        var classEntity = await context.Classes
            .Include(c => c.Course)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == classId);

        if (classEntity == null) return null;

        if (currentUserRole == "Instructor" && classEntity.InstructorId != currentUserId)
        {
            throw new UnauthorizedAccessException();
        }
        if (currentUserRole == "Student" && !classEntity.Enrollments.Any(e => e.StudentId == currentUserId))
        {
            throw new UnauthorizedAccessException();
        }

        var students = await context.ClassEnrollments
            .Include(e => e.Student)
            .Where(e => e.ClassId == classId)
            .Select(e => new ClassStudentDto(
                e.Student.Id,
                e.Student.FullName,
                e.Student.Email,
                e.Student.UserCode ?? "Chưa cấp mã",
                e.EnrollmentDate.ToString("dd/MM/yyyy HH:mm")
            ))
            .ToListAsync();

        return new ClassDetailsResponseDto(
            classEntity.Id,
            classEntity.ClassCode,
            classEntity.ClassName,
            classEntity.Course?.Title ?? "Không rõ khóa học",
            classEntity.GoogleMeetLink,
            classEntity.AcademicYear,
            students
        );
    }

    public async Task<IEnumerable<StudentClassResponseDto>> GetClassesByStudentAsync(Guid studentId)
    {
        return await context.ClassEnrollments
            .Include(e => e.Class)
            .ThenInclude(c => c.Course)
            .Where(e => e.StudentId == studentId)
            .Select(e => new StudentClassResponseDto(
                e.Class.Id,
                e.Class.CourseId,
                e.Class.ClassCode,
                e.Class.ClassName,
                e.Class.Course != null ? e.Class.Course.Title : "Chưa rõ môn",
                e.Class.AcademicYear,
                e.Class.GoogleMeetLink
            ))
            .ToListAsync();
    }

    public async Task<int> GetStudentCountAsync(Guid classId)
    {
        var classExists = await context.Classes.AnyAsync(c => c.Id == classId);
        if (!classExists) throw new KeyNotFoundException("Không tìm thấy lớp học.");

        return await context.ClassEnrollments.CountAsync(e => e.ClassId == classId);
    }

    public async Task<(int AddedCount, List<string> Errors)> ImportStudentsFromExcelAsync(Guid classId, IFormFile file)
    {
        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");
        var addedCount = 0;
        var errorRows = new List<string>();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
            var code = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
            var email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(email)) continue;

            var student = await context.Users.FirstOrDefaultAsync(u => u.UserCode == code || u.Email == email);

            if (student == null)
            {
                errorRows.Add($"Dòng {row}: Không tìm thấy Sinh viên '{code ?? email}' trong hệ thống.");
                continue;
            }

            var isEnrolled = await context.ClassEnrollments.AnyAsync(e => e.ClassId == classId && e.StudentId == student.Id);
            if (!isEnrolled)
            {
                await EnrollStudentAsync(classId, student.Id);
                addedCount++;
            }
        }

        return (addedCount, errorRows);
    }

    public async Task<bool> RemoveStudentFromClassAsync(Guid classId, Guid studentId)
    {
        var enrollment = await context.ClassEnrollments
            .FirstOrDefaultAsync(e => e.ClassId == classId && e.StudentId == studentId);

        if (enrollment == null) return false;

        context.ClassEnrollments.Remove(enrollment);
        await context.SaveChangesAsync();

        return true;
    }
}