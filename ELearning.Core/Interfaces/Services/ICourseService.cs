using ELearning.Core.DTOs.Course;

namespace ELearning.Core.Interfaces.Services;

public interface ICourseService
{
    Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync(Guid? instructorId = null);
    Task<CourseResponseDto?> GetCourseByIdAsync(Guid id);
    Task<bool> IsCourseCreatorOrAdminAsync(Guid courseId, Guid userId, string role);
    Task<bool> CheckCourseAccessAsync(Guid courseId, Guid currentUserId, string currentUserRole);
    Task<CourseResponseDto> CreateCourseAsync(CreateCourseRequestDto request, Guid creatorId);
    Task<bool> UpdateCourseAsync(Guid id, UpdateCourseRequestDto request);
    Task<bool> DeleteCourseAsync(Guid id);
    Task<IEnumerable<AssignmentDto>> GetAssignmentsByCourseAsync(Guid courseId);
    Task<CourseResponseDto?> CopyCourseAsync(Guid courseId, Guid newCreatorId);
}