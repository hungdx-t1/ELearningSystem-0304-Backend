using ELearning.API.Controllers;
using ELearning.Core.DTOs.Auth;
using ELearning.Core.DTOs.User;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ELearning.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    // Helper method để giả lập việc user đã đăng nhập (có chứa Token Claim)
    private void SetUserClaim(Guid userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequestDto("test@gmail.com", "123456");
        var mockUser = new UserResponseDto(Guid.NewGuid(), "STU-001", "Test", "test@gmail.com", UserRole.Student, null, null, null, true, DateTime.UtcNow);
        var expectedResult = new LoginResponseDto("mock_jwt_token", mockUser);

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                        .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedResult, okResult.Value);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto("wrong@gmail.com", "123456");
        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                        .ReturnsAsync((LoginResponseDto?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var responseValue = unauthorizedResult.Value?.GetType().GetProperty("message")?.GetValue(unauthorizedResult.Value, null);
        Assert.Equal("Email hoặc Mật khẩu không chính xác!", responseValue);
    }

    [Fact]
    public async Task ForgotPassword_Always_ReturnsOkMessage()
    {
        // Arrange
        var request = new ForgotPasswordDto { Email = "test@gmail.com" };
        _mockAuthService.Setup(s => s.ForgotPasswordAsync(request)).ReturnsAsync(true);

        // Act
        var result = await _controller.ForgotPassword(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task VerifyOtp_ValidOtp_ReturnsOkWithToken()
    {
        // Arrange
        var request = new VerifyOtpDto { Email = "test@gmail.com", OtpCode = "123456" };
        _mockAuthService.Setup(s => s.VerifyOtpAsync(It.IsAny<VerifyOtpDto>()))
                        .ReturnsAsync("reset_token_xyz");

        // Act
        var result = await _controller.VerifyOtp(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseToken = okResult.Value?.GetType().GetProperty("resetToken")?.GetValue(okResult.Value, null);
        Assert.Equal("reset_token_xyz", responseToken);
    }

    [Fact]
    public async Task VerifyOtp_InvalidOtp_ReturnsBadRequest()
    {
        // Arrange
        var request = new VerifyOtpDto { Email = "test@gmail.com", OtpCode = "000000" };
        _mockAuthService.Setup(s => s.VerifyOtpAsync(It.IsAny<VerifyOtpDto>()))
                        .ReturnsAsync((string?)null);

        // Act
        var result = await _controller.VerifyOtp(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Success_ReturnsOk()
    {
        // Arrange
        var request = new ResetPasswordDto { ResetToken = "xyz", NewPassword = "NewPassword123" };
        _mockAuthService.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
                        .ReturnsAsync(true);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ResetPassword_Failure_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResetPasswordDto { ResetToken = "invalid_token", NewPassword = "NewPassword123" };
        _mockAuthService.Setup(s => s.ResetPasswordAsync(It.IsAny<ResetPasswordDto>()))
                        .ReturnsAsync(false);

        // Act
        var result = await _controller.ResetPassword(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RequestChangeEmail_UserNotAuthorized_ReturnsUnauthorized()
    {
        // Arrange
        // Không gọi SetUserClaim() để giả lập trường hợp user mất token hoặc token hỏng
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        var request = new RequestChangeEmailDto { NewEmail = "new@gmail.com" };

        // Act
        var result = await _controller.RequestChangeEmail(request);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task RequestChangeEmail_ValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserClaim(userId); // Giả lập user đã đăng nhập hợp lệ
        var request = new RequestChangeEmailDto { NewEmail = "new@gmail.com" };

        _mockAuthService.Setup(s => s.RequestChangeEmailAsync(request, userId))
                        .ReturnsAsync(true);

        // Act
        var result = await _controller.RequestChangeEmail(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task ConfirmChangeEmail_ValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserClaim(userId);
        var request = new ConfirmChangeEmailDto { NewEmail = "new@gmail.com", OtpCode = "123456" };

        _mockAuthService.Setup(s => s.ConfirmChangeEmailAsync(request, userId))
                        .ReturnsAsync(true);

        // Act
        var result = await _controller.ConfirmChangeEmail(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}