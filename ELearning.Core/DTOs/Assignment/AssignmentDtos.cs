namespace ELearning.Core.DTOs.Assignment;

// dữ liệu trả về cho Angular/Flutter (Ví dụ: Không trả về các thông tin nhạy cảm)
public record CourseResponseDto(
    Guid Id,
    string Title,
    string? Description,
    Guid? InstructorId,
    string? ThumbnailUrl,
    DateTime CreatedAt
);

// dữ liệu Frontend gửi lên khi Tạo mới Khóa học
public record CreateCourseRequestDto(
    string Title,
    string? Description,
    Guid? InstructorId,
    string? ThumbnailUrl
);

// dữ liệu Frontend gửi lên khi Cập nhật Khóa học
public record UpdateCourseRequestDto(
    string Title,
    string? Description,
    string? ThumbnailUrl
);