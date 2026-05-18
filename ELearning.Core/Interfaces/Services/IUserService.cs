using ELearning.Core.DTOs;
using ELearning.Core.DTOs.User;

namespace ELearning.Core.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    Task<PagedResult<UserResponseDto>> GetUsersPaginatedAsync(string? search, string? role, int page, int pageSize);
    Task<UserResponseDto?> GetUserByIdAsync(Guid id);
    Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);
    Task<bool> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> ToggleUserStatusAsync(Guid id);
    Task<(int SuccessCount, List<string> Errors)> ImportUsersFromExcelAsync(Stream excelStream);
    Task<byte[]> ExportUsersToExcelAsync();
}