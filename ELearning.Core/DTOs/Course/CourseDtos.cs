namespace ELearning.Core.DTOs.Course;

public record CourseResponseDto(Guid Id, string Title, string? Description, string? ThumbnailUrl, DateTime CreatedAt, Guid? CreatorId, string? CreatorName, bool IsPublic);
public record CreateCourseRequestDto(string Title, string? Description, string? ThumbnailUrl, bool IsPublic = false);
public record UpdateCourseRequestDto(string Title, string? Description, string? ThumbnailUrl, bool IsPublic);

public record AssignmentDto(Guid Id, string Title, string ChapterName);