using ELearning.Core.DTOs.Auth;

namespace ELearning.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<string?> VerifyOtpAsync(VerifyOtpDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<bool> RequestChangeEmailAsync(RequestChangeEmailDto dto, Guid userId);
    Task<bool> ConfirmChangeEmailAsync(ConfirmChangeEmailDto dto, Guid userId);
    Task<bool> RequestChangePasswordAsync(RequestChangePasswordDto dto, Guid userId);
    Task<bool> ConfirmChangePasswordAsync(ConfirmChangePasswordDto dto, Guid userId);
}