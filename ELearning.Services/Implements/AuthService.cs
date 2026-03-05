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

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IConfiguration _configuration; // Để đọc appsettings.json

    public AuthService(IGenericRepository<User> userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // 1. Kiểm tra Email có tồn tại không
        var users = await _userRepository.FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();

        // 2. Kiểm tra Pass (TODO: Sau này bạn nhớ dùng BCrypt.Verify() chỗ này nhé)
        if (user == null || user.PasswordHash != request.Password)
        {
            return null; // Trả về null nếu sai email hoặc pass
        }

        // 3. Nếu đúng, bắt đầu tạo Thẻ (Token)
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);

        // 4. Nhét thông tin cá nhân (Claims) vào trong thẻ
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()) // Rất quan trọng để phân quyền
        };

        // 5. Cấu hình thẻ (Thời hạn, thuật toán mã hóa)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!)),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // 6. Đúc thẻ thành chuỗi Text
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        var userDto = new UserResponseDto(user.Id, user.UserCode, user.FullName, user.Email, user.Role, user.AvatarUrl, user.DateOfBirth, user.AdministrativeClass, user.IsActive, user.CreatedAt);
        
        return new LoginResponseDto(jwtString, userDto);
    }
}