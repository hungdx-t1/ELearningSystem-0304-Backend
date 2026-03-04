using ELearning.Core.DTOs.Course;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;

namespace ELearning.Services.Implements;

public class CourseService : ICourseService
{
    private readonly IGenericRepository<Course> _courseRepository;

    public CourseService(IGenericRepository<Course> courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        
        // Map từ Entity sang DTO
        return courses.Select(c => new CourseResponseDto(
            c.Id, c.Title, c.Description, c.InstructorId, c.ThumbnailUrl, c.CreatedAt));
    }

    public async Task<CourseResponseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null) return null;

        return new CourseResponseDto(
            course.Id, course.Title, course.Description, course.InstructorId, course.ThumbnailUrl, course.CreatedAt);
    }

    public async Task<CourseResponseDto> CreateCourseAsync(CreateCourseRequestDto request)
    {
        // Map từ DTO sang Entity để lưu DB
        var newCourse = new Course
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            InstructorId = request.InstructorId,
            ThumbnailUrl = request.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(newCourse);
        await _courseRepository.SaveChangesAsync();

        return new CourseResponseDto(
            newCourse.Id, newCourse.Title, newCourse.Description, newCourse.InstructorId, newCourse.ThumbnailUrl, newCourse.CreatedAt);
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
}