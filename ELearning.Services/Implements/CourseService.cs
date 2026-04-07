using ELearning.Core.DTOs.Course;
using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class CourseService : ICourseService
{
    private readonly AppDbContext _context;
    private readonly IGenericRepository<Course> _courseRepository;

    public CourseService(IGenericRepository<Course> courseRepository, AppDbContext context)
    {
        _courseRepository = courseRepository;
        _context = context;
    }

    public async Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync()
    {
        // Dùng Include để join sang bảng User lấy tên người tạo
        var courses = await _context.Courses
            .Include(c => c.Creator) 
            .ToListAsync();

        return courses.Select(c => new CourseResponseDto(
            c.Id, 
            c.Title, 
            c.Description, 
            c.ThumbnailUrl, 
            c.CreatedAt, 
            c.CreatorId, 
            c.Creator?.FullName // Lấy tên Giảng viên
        ));
    }

    public async Task<CourseResponseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await _context.Courses
            .Include(c => c.Creator)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return null;

        return new CourseResponseDto(
            course.Id, 
            course.Title, 
            course.Description, 
            course.ThumbnailUrl, 
            course.CreatedAt, 
            course.CreatorId, 
            course.Creator?.FullName
        );
    }

    // 🌟 Nhận thêm creatorId từ Controller
    public async Task<CourseResponseDto> CreateCourseAsync(CreateCourseRequestDto request, Guid creatorId)
    {
        var newCourse = new Course
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow,
            CreatorId = creatorId // Gán người tạo
        };

        await _courseRepository.AddAsync(newCourse);
        await _courseRepository.SaveChangesAsync();

        // Lúc mới tạo trả về luôn, tên người tạo có thể truyền null (trên UI thường tự biết là mình)
        return new CourseResponseDto(newCourse.Id, newCourse.Title, newCourse.Description, newCourse.ThumbnailUrl, newCourse.CreatedAt, newCourse.CreatorId, null);
    }

    public async Task<bool> UpdateCourseAsync(Guid id, UpdateCourseRequestDto request)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        course.Title = request.Title;
        course.Description = request.Description;
        course.ThumbnailUrl = request.ThumbnailUrl;

        _courseRepository.Update(course);
        return await _courseRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        _courseRepository.Delete(course);
        return await _courseRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<AssignmentDto>> GetAssignmentsByCourseAsync(Guid courseId)
    {
        var assignments = await _context.Lessons
            .Include(l => l.Chapter)
            .Where(l => l.Chapter.CourseId == courseId && l.Type == LessonType.Assignment)
            .Select(l => new AssignmentDto(
                l.Id, 
                l.Title, 
                l.Chapter.Title
            ))
            .ToListAsync();

        return assignments;
    }
}