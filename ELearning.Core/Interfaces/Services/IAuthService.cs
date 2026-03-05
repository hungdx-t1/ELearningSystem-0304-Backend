using ELearning.Core.DTOs.Auth;

namespace ELearning.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}