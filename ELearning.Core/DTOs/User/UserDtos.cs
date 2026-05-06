using ELearning.Core.Enums;
using System.ComponentModel.DataAnnotations;
using ELearning.Core.Common.Attributes;
using ELearning.Core.Common.Constants;

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
    [Required(ErrorMessage = "Mã người dùng không được để trống")] string UserCode,
    [Required(ErrorMessage = "Họ tên không được để trống")] string FullName,

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Sai định dạng Email")]
    [AllowedEmailDomain("gmail.com", "outlook.com", "outlook.com.vn")]
    string Email,

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [RegularExpression(ValidationConstants.PasswordRegexPattern, ErrorMessage = ValidationConstants.PasswordErrorMessage)]
    string Password,

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