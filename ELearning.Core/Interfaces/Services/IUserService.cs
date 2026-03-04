using ELearning.Core.DTOs.User;

namespace ELearning.Core.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);
    Task<bool> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
    Task<bool> DeleteUserAsync(Guid id);
}