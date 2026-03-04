using ELearning.Core.DTOs.User;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;

namespace ELearning.Services.Implements;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _userRepository;

    public UserService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        
        return users.Select(u => new UserResponseDto(
            u.Id, u.UserCode, u.FullName, u.Email, u.Role, 
            u.AvatarUrl, u.DateOfBirth, u.AdministrativeClass, u.IsActive, u.CreatedAt
        ));
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserResponseDto(
            user.Id, user.UserCode, user.FullName, user.Email, user.Role, 
            user.AvatarUrl, user.DateOfBirth, user.AdministrativeClass, user.IsActive, user.CreatedAt
        );
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
    {
        // TODO: Trong thực tế, bạn phải dùng BCrypt để Hash cái request.Password này
        // Tạm thời để string thuần để test chức năng trước
        string hashedPassword = request.Password; 

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            UserCode = request.UserCode,
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = hashedPassword,
            Role = request.Role,
            AdministrativeClass = request.AdministrativeClass,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        return new UserResponseDto(
            newUser.Id, newUser.UserCode, newUser.FullName, newUser.Email, newUser.Role, 
            newUser.AvatarUrl, newUser.DateOfBirth, newUser.AdministrativeClass, newUser.IsActive, newUser.CreatedAt
        );
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        // Chỉ cập nhật những trường được phép
        user.FullName = request.FullName;
        user.AvatarUrl = request.AvatarUrl;
        user.DateOfBirth = request.DateOfBirth;
        user.AdministrativeClass = request.AdministrativeClass;
        user.IsActive = request.IsActive;

        _userRepository.Update(user);
        return await _userRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        _userRepository.Delete(user);
        return await _userRepository.SaveChangesAsync();
    }
}