namespace ELearning.Core.DTOs.Course;

public record CourseResponseDto(Guid Id, string Title, string? Description, string? ThumbnailUrl, DateTime CreatedAt);
public record CreateCourseRequestDto(string Title, string? Description, string? ThumbnailUrl);
public record UpdateCourseRequestDto(string Title, string? Description, string? ThumbnailUrl);