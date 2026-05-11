using ELearning.Core.DTOs.Auth;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Services.Implements;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ELearning.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IGenericRepository<User>> _mockUserRepo;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Khởi tạo các đối tượng giả (Mock)
        _mockUserRepo = new Mock<IGenericRepository<User>>();
        _mockConfig = new Mock<IConfiguration>();
        _mockEmailService = new Mock<IEmailService>();

        // Truyền các đối tượng giả vào AuthService thực tế
        _authService = new AuthService(
            _mockUserRepo.Object,
            _mockConfig.Object,
            _mockEmailService.Object
        );
    }

    [Fact]
    public async Task ForgotPasswordAsync_UserExists_ReturnsTrueAndSendsEmail()
    {
        // Chuẩn bị dữ liệu
        var testEmail = "sinhvien@gmail.com";
        var fakeUser = new User { Id = Guid.NewGuid(), Email = testEmail };

        // Giả lập: Khi gọi hàm FindAsync tìm Email, sẽ trả về fakeUser này
        _mockUserRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(new List<User> { fakeUser });

        var requestDto = new ForgotPasswordDto { Email = testEmail };

        // Hành động
        var result = await _authService.ForgotPasswordAsync(requestDto);

        // Kiểm chứng
        Assert.True(result); // Phải trả về true

        // Xác minh xem Repository có được gọi hàm Update và SaveChanges không?
        _mockUserRepo.Verify(repo => repo.Update(It.IsAny<User>()), Times.Once);
        _mockUserRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        // Xác minh xem EmailService có thực sự được kích hoạt để gửi OTP không?
        _mockEmailService.Verify(email =>
            email.SendEmailAsync(testEmail, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WrongEmail_ReturnsNull()
    {
        // ARRANGE
        // Giả lập: Tìm email không thấy (trả về danh sách rỗng)
        _mockUserRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(new List<User>());

        var requestDto = new LoginRequestDto("wrong@gmail.com", "123456");

        // ACT
        var result = await _authService.LoginAsync(requestDto);

        // ASSERT
        Assert.Null(result); // Đăng nhập sai email phải trả về null
    }
}