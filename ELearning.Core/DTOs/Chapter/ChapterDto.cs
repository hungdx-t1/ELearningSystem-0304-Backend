namespace ELearning.Core.DTOs.Chapter;

public record ChapterResponseDto(
    Guid Id,
    Guid CourseId,
    string Title,
    int SortOrder
);

public record CreateChapterRequestDto(
    Guid CourseId,
    string Title,
    int SortOrder = 0 // Mặc định là 0 để hệ thống tự tính
);

public record UpdateChapterRequestDto(
    string Title,
    int SortOrder
);