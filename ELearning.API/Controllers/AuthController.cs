using ELearning.Core.DTOs.Auth;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [EndpointSummary("Đăng nhập hệ thống")]
    [EndpointDescription("Xác thực tài khoản người dùng và cấp phát JWT Token.")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(new { message = "Email hoặc Mật khẩu không chính xác!" });

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [EndpointSummary("Quên mật khẩu")]
    [EndpointDescription("Gửi yêu cầu reset mật khẩu qua email kèm mã OTP.")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        await authService.ForgotPasswordAsync(request);
        return Ok(new { message = "Nếu email hợp lệ, một mã OTP đã được gửi đến hộp thư của bạn." });
    }

    [HttpPost("verify-otp")]
    [EndpointSummary("Xác minh OTP")]
    [EndpointDescription("Kiểm tra mã OTP để lấy token đổi mật khẩu.")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
    {
        var resetToken = await authService.VerifyOtpAsync(request);

        if (resetToken == null)
            return BadRequest(new { message = "Mã OTP không chính xác hoặc đã hết hạn!" });

        return Ok(new { message = "Xác minh OTP thành công.", resetToken });
    }

    [HttpPost("reset-password")]
    [EndpointSummary("Đặt lại mật khẩu")]
    [EndpointDescription("Sử dụng token xác thực để thay đổi mật khẩu mới.")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var isSuccess = await authService.ResetPasswordAsync(request);

        if (!isSuccess)
            return BadRequest(new { message = "Yêu cầu đổi mật khẩu không hợp lệ hoặc đã hết hạn!" });

        return Ok(new { message = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại." });
    }

    [Authorize]
    [HttpPost("request-change-email")]
    [EndpointSummary("Yêu cầu đổi email")]
    [EndpointDescription("Gửi yêu cầu thay đổi email kèm mã OTP.")]
    public async Task<IActionResult> RequestChangeEmail([FromBody] RequestChangeEmailDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var isSuccess = await authService.RequestChangeEmailAsync(request, userId);
        if (!isSuccess) return BadRequest(new { message = "Yêu cầu thất bại. Email có thể đã được sử dụng." });

        return Ok(new { message = "Mã OTP đã được gửi đến email mới của bạn." });
    }

    [Authorize]
    [HttpPost("confirm-change-email")]
    [EndpointSummary("Xác nhận đổi email")]
    [EndpointDescription("Xác nhận thay đổi email kèm mã OTP.")]
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailDto request)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid userId)) return Unauthorized();

        var isSuccess = await authService.ConfirmChangeEmailAsync(request, userId);
        if (!isSuccess) return BadRequest(new { message = "Mã OTP không chính xác hoặc đã hết hạn!" });

        return Ok(new { message = "Đổi email thành công. Vui lòng dùng email mới ở lần đăng nhập tiếp theo." });
    }
}