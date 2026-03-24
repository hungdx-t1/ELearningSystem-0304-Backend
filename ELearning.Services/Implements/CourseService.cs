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
        var courses = await _courseRepository.GetAllAsync();
        return courses.Select(c => new CourseResponseDto(c.Id, c.Title, c.Description, c.ThumbnailUrl, c.CreatedAt));
    }

    public async Task<CourseResponseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return null;
        return new CourseResponseDto(course.Id, course.Title, course.Description, course.ThumbnailUrl, course.CreatedAt);
    }

    public async Task<CourseResponseDto> CreateCourseAsync(CreateCourseRequestDto request)
    {
        var newCourse = new Course
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(newCourse);
        await _courseRepository.SaveChangesAsync();

        return new CourseResponseDto(newCourse.Id, newCourse.Title, newCourse.Description, newCourse.ThumbnailUrl, newCourse.CreatedAt);
    }

    public async Task<bool> UpdateCourseAsync(Guid id, UpdateCourseRequestDto request)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        // Cập nhật các trường
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