namespace ELearning.Core.DTOs.Course;

public record CourseResponseDto(Guid Id, string Title, string? Description, string? ThumbnailUrl, DateTime CreatedAt, Guid? CreatorId, string? CreatorName);
public record CreateCourseRequestDto(string Title, string? Description, string? ThumbnailUrl);
public record UpdateCourseRequestDto(string Title, string? Description, string? ThumbnailUrl);

public record AssignmentDto(Guid Id, string Title, string ChapterName);