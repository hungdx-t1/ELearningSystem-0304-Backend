using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Core.DTOs.Class;

namespace ELearning.Services.Implements;

public class ClassService : IClassService
{
    private readonly IGenericRepository<Class> _classRepo;
    private readonly IGenericRepository<ClassEnrollment> _enrollmentRepo;

    public ClassService(IGenericRepository<Class> classRepo, IGenericRepository<ClassEnrollment> enrollmentRepo)
    {
        _classRepo = classRepo;
        _enrollmentRepo = enrollmentRepo;
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync()
    {
        var classes = await _classRepo.GetAllAsync();
        
        // Cập nhật DTO: Bơm thêm CourseId và InstructorId (Lưu ý: InstructorId giờ có thể null nếu GV bị xóa)
        return classes.Select(c => new ClassResponseDto(
            c.Id, 
            c.CourseId, 
            c.ClassCode, 
            c.ClassName, 
            c.InstructorId ?? Guid.Empty, // Tránh lỗi null nếu GV đã nghỉ việc
            c.GoogleMeetLink, 
            c.AcademicYear, 
            c.Description
        ));
    }

    public async Task<ClassResponseDto> CreateClassAsync(CreateClassRequestDto request)
    {
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
        
        await _classRepo.AddAsync(newClass);
        await _classRepo.SaveChangesAsync();
        
        return new ClassResponseDto(
            newClass.Id, 
            newClass.CourseId, 
            newClass.ClassCode, 
            newClass.ClassName, 
            newClass.InstructorId ?? Guid.Empty, 
            newClass.GoogleMeetLink, 
            newClass.AcademicYear, 
            newClass.Description
        );
    }

    public async Task<bool> EnrollStudentAsync(Guid classId, Guid studentId)
    {
        // Kiểm tra xem đã ghi danh chưa để tránh lỗi Duplicate
        var exists = await _enrollmentRepo.FindAsync(e => e.ClassId == classId && e.StudentId == studentId);
        if (exists.Any()) return true; // Đã vào lớp rồi

        var enrollment = new ClassEnrollment
        {
            ClassId = classId,
            StudentId = studentId,
            EnrollmentDate = DateTime.UtcNow
        };
        await _enrollmentRepo.AddAsync(enrollment);
        return await _enrollmentRepo.SaveChangesAsync();
    }
}