using ELearning.Core.DTOs.Auth;
using ELearning.Core.DTOs.User;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ELearning.Services.Implements;

public class AuthService(IGenericRepository<User> userRepository, IConfiguration configuration, IEmailService emailService) : IAuthService
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // 1. Kiểm tra Email có tồn tại không
        var users = await userRepository.FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();

        // 2. Kiểm tra Pass (TODO: Sau này bạn nhớ dùng BCrypt.Verify() chỗ này nhé)
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null; // Trả về null nếu sai email hoặc pass
        }

        // 3. Nếu đúng, bắt đầu tạo Thẻ (Token)
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!);

        // 4. Nhét thông tin cá nhân (Claims) vào trong thẻ
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()) // Rất quan trọng để phân quyền
        };

        // 5. Cấu hình thẻ (Thời hạn, thuật toán mã hóa)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(configuration["JwtSettings:ExpiryMinutes"]!)),
            Issuer = configuration["JwtSettings:Issuer"],
            Audience = configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // 6. Đúc thẻ thành chuỗi Text
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        var userDto = new UserResponseDto(user.Id, user.UserCode, user.FullName, user.Email, user.Role, user.AvatarUrl, user.DateOfBirth, user.AdministrativeClass, user.IsActive, user.CreatedAt);

        return new LoginResponseDto(jwtString, userDto);
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var users = await userRepository.FindAsync(u => u.Email == dto.Email);
        var user = users.FirstOrDefault();

        // Luôn trả về true để ngăn hacker dò tìm email
        if (user == null) return true;

        var otp = new Random().Next(100000, 999999).ToString();
        user.OtpCode = otp;
        user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        var body = $@"
            <h3>Khôi phục mật khẩu LMS</h3>
            <p>Mã OTP của bạn là: <strong>{otp}</strong></p>
            <p>Mã này có hiệu lực trong vòng 5 phút. Vui lòng không chia sẻ cho bất kỳ ai.</p>";

        await emailService.SendEmailAsync(user.Email, "LMS OTP Khôi Phục Mật Khẩu", body);
        return true;
    }

    public async Task<string?> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var users = await userRepository.FindAsync(u => u.Email == dto.Email);
        var user = users.FirstOrDefault();

        if (user == null || user.OtpCode != dto.OtpCode || DateTime.UtcNow > user.OtpExpiryTime)
            return null; // OTP sai hoặc đã hết hạn

        // OTP đúng, thu hồi OTP, sinh Token đi tiếp
        user.OtpCode = null;
        user.OtpExpiryTime = null;

        var resetToken = Guid.NewGuid().ToString();
        user.ResetToken = resetToken;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        return resetToken;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var users = await userRepository.FindAsync(u => u.ResetToken == dto.ResetToken);
        var user = users.FirstOrDefault();

        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.ResetToken = null; // Thu hồi Token

        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<bool> RequestChangeEmailAsync(RequestChangeEmailDto dto, Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        var existingUsers = await userRepository.FindAsync(u => u.Email == dto.NewEmail);
        if (existingUsers.Any()) return false; // Email mới đã có người dùng

        var otp = new Random().Next(100000, 999999).ToString();
        user.OtpCode = otp;
        user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
        user.PendingNewEmail = dto.NewEmail;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();

        var body = $@"
            <h3>Xác thực Đổi Email LMS</h3>
            <p>Bạn đã yêu cầu đổi email sang địa chỉ này. Mã OTP của bạn là: <strong>{otp}</strong></p>
            <p>Mã này có hiệu lực trong vòng 5 phút.</p>";

        await emailService.SendEmailAsync(dto.NewEmail, "LMS OTP Đổi Email", body);
        return true;
    }

    public async Task<bool> ConfirmChangeEmailAsync(ConfirmChangeEmailDto dto, Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null || user.PendingNewEmail != dto.NewEmail) return false;

        if (user.OtpCode != dto.OtpCode || DateTime.UtcNow > user.OtpExpiryTime)
            return false;

        // Tiến hành cập nhật email mới
        user.Email = user.PendingNewEmail;
        user.PendingNewEmail = null;
        user.OtpCode = null;
        user.OtpExpiryTime = null;

        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }
}