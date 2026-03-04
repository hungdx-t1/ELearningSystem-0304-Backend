using ELearning.Core.Enums;

namespace ELearning.Core.DTOs.User;

// 1. Dữ liệu trả về (Lưu ý: Tuyệt đối không trả về PasswordHash)
public record UserResponseDto(
    Guid Id,
    string UserCode,
    string FullName,
    string Email,
    UserRole Role,
    string? AvatarUrl,
    DateTime? DateOfBirth,
    string? AdministrativeClass,
    bool IsActive,
    DateTime CreatedAt
);

// 2. Dữ liệu gửi lên khi tạo tài khoản
public record CreateUserRequestDto(
    string UserCode,       // Ví dụ: STU2026001
    string FullName, 
    string Email, 
    string Password,       // Pass gốc từ form đăng ký (Backend sẽ mã hóa sau)
    UserRole Role,
    string? AdministrativeClass 
);

// 3. Dữ liệu gửi lên khi Admin/User cập nhật profile
// Không cho phép sửa Email và UserCode để tránh xung đột dữ liệu
public record UpdateUserRequestDto(
    string FullName,
    string? AvatarUrl,
    DateTime? DateOfBirth,
    string? AdministrativeClass,
    bool IsActive
);