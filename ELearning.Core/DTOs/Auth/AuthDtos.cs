using ELearning.Core.DTOs.User;
using ELearning.Core.Enums;

namespace ELearning.Core.DTOs.Auth;

public record LoginRequestDto(string Email, string Password);

// Trả về Token kèm theo thông tin User để Frontend hiển thị Avatar, Tên...
public record LoginResponseDto(string Token, UserResponseDto User);

// Dữ liệu người dùng gửi lên để tạo tài khoản
public record RegisterRequestDto(
    string Email, 
    string Password, 
    string FullName, 
    UserRole Role = UserRole.Student // Sửa string thành UserRole
);

public record RequestChangePasswordDto(string OldPassword);
public record ConfirmChangePasswordDto(string NewPassword, string OtpCode);