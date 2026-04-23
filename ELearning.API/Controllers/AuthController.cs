using ELearning.Core.DTOs.Auth;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        
        if (result == null)
            return Unauthorized(new { message = "Email hoặc Mật khẩu không chính xác!" });

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = "Nếu email hợp lệ, một mã OTP đã được gửi đến hộp thư của bạn." });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
    {
        var resetToken = await _authService.VerifyOtpAsync(request);
        
        if (resetToken == null)
            return BadRequest(new { message = "Mã OTP không chính xác hoặc đã hết hạn!" });

        return Ok(new { message = "Xác minh OTP thành công.", resetToken = resetToken });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var isSuccess = await _authService.ResetPasswordAsync(request);
        
        if (!isSuccess)
            return BadRequest(new { message = "Yêu cầu đổi mật khẩu không hợp lệ hoặc đã hết hạn!" });

        return Ok(new { message = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại." });
    }

    [Authorize]
    [HttpPost("request-change-email")]
    public async Task<IActionResult> RequestChangeEmail([FromBody] RequestChangeEmailDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var isSuccess = await _authService.RequestChangeEmailAsync(request, userId);
        if (!isSuccess) return BadRequest(new { message = "Yêu cầu thất bại. Email có thể đã được sử dụng." });

        return Ok(new { message = "Mã OTP đã được gửi đến email mới của bạn." });
    }

    [Authorize]
    [HttpPost("confirm-change-email")]
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var isSuccess = await _authService.ConfirmChangeEmailAsync(request, userId);
        if (!isSuccess) return BadRequest(new { message = "Mã OTP không chính xác hoặc đã hết hạn!" });

        return Ok(new { message = "Đổi email thành công. Vui lòng dùng email mới ở lần đăng nhập tiếp theo." });
    }
}