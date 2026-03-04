using ELearning.Core.DTOs.Course;

namespace ELearning.Core.Interfaces.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync();
    Task<CourseResponseDto?> GetCourseByIdAsync(Guid id);
    Task<CourseResponseDto> CreateCourseAsync(CreateCourseRequestDto request);
    Task<bool> UpdateCourseAsync(Guid id, UpdateCourseRequestDto request);
    Task<bool> DeleteCourseAsync(Guid id);
}