using ELearning.Core.Enums;

namespace ELearning.Core.DTOs.Lesson;

// 1. Dữ liệu trả về cho Frontend
public record LessonResponseDto(
    Guid Id,
    Guid ChapterId,
    string Title,
    LessonType Type,
    VideoProvider? VideoProvider,
    string? VideoUrl,
    string? DocumentUrl,
    int? Duration,
    int SortOrder
);

// 2. Dữ liệu Frontend gửi lên khi Giảng viên tạo bài học mới
public record CreateLessonRequestDto(
    Guid ChapterId, // Bắt buộc phải biết bài học này thuộc chương nào
    string Title,
    LessonType Type,
    VideoProvider? VideoProvider,
    string? VideoUrl,
    string? DocumentUrl,
    int? Duration,
    int SortOrder
);

// 3. Dữ liệu Frontend gửi lên khi cập nhật (Không cho phép đổi ChapterId)
public record UpdateLessonRequestDto(
    string Title,
    LessonType Type,
    VideoProvider? VideoProvider,
    string? VideoUrl,
    string? DocumentUrl,
    int? Duration,
    int SortOrder
);