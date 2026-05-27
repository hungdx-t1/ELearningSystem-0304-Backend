using ELearning.Core.DTOs.User;
using ELearning.Core.Enums;
using System.ComponentModel.DataAnnotations;
using ELearning.Core.Common.Constants;

namespace ELearning.Core.DTOs.Auth;

public record LoginRequestDto(string Email, string Password);

// Trả về Token kèm theo thông tin User để Frontend hiển thị Avatar, Tên...
public record LoginResponseDto(string Token, UserResponseDto User);

// Dữ liệu người dùng gửi lên để tạo tài khoản
public record RegisterRequestDto(
    [Required][EmailAddress] string Email,
    
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên.")]
    [RegularExpression(ValidationConstants.PasswordRegexPattern, ErrorMessage = ValidationConstants.PasswordErrorMessage)]
    string Password,
    
    [Required] string FullName,
    UserRole Role = UserRole.Student // Sửa string thành UserRole
);

public record RequestChangePasswordDto(string OldPassword);


public record ConfirmChangePasswordDto(
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên.")]
    [RegularExpression(ValidationConstants.PasswordRegexPattern, ErrorMessage = ValidationConstants.PasswordErrorMessage)]
    string NewPassword, 
    
    [Required] string OtpCode
);