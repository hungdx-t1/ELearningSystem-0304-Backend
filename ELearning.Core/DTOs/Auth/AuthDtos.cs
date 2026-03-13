using ELearning.Core.DTOs.User;

namespace ELearning.Core.DTOs.Auth;

public record LoginRequestDto(string Email, string Password);

// Trả về Token kèm theo thông tin User để Frontend hiển thị Avatar, Tên...
public record LoginResponseDto(string Token, UserResponseDto User);

// Dữ liệu người dùng gửi lên để tạo tài khoản
public record RegisterRequestDto(
    string Email, 
    string Password, 
    string FullName, 
    string Role = "Student" // Mặc định tạo ra là Sinh viên
);