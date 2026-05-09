using ELearning.Core.DTOs;
using ELearning.Core.DTOs.User;
using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class UserService(IGenericRepository<User> userRepository, AppDbContext context) : IUserService
{
    public async Task<PagedResult<UserResponseDto>> GetUsersPaginatedAsync(string? search, string? role, int page, int pageSize)
    {
        var query = context.Users.AsQueryable();

        // Lọc theo tên hoặc email
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) || u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Lọc theo Role
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, out var parsedRole))
        {
            query = query.Where(u => u.Role == parsedRole);
        }

        // Đếm tổng số lượng (trước khi cắt trang)
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Cắt trang (Phép thuật nằm ở Skip và Take)
        var users = await query
            .OrderByDescending(u => u.CreatedAt) // Sắp xếp mới nhất lên đầu
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = users.Select(u => new UserResponseDto(
            u.Id, u.UserCode, u.FullName, u.Email, u.Role,
            u.AvatarUrl, u.DateOfBirth, u.AdministrativeClass, u.IsActive, u.CreatedAt
        ));

        return new PagedResult<UserResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        var users = await userRepository.GetAllAsync();

        return users.Select(u => new UserResponseDto(
            u.Id, u.UserCode, u.FullName, u.Email, u.Role,
            u.AvatarUrl, u.DateOfBirth, u.AdministrativeClass, u.IsActive, u.CreatedAt
        ));
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserResponseDto(
            user.Id, user.UserCode, user.FullName, user.Email, user.Role,
            user.AvatarUrl, user.DateOfBirth, user.AdministrativeClass, user.IsActive, user.CreatedAt
        );
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
    {
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

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

        await userRepository.AddAsync(newUser);
        await userRepository.SaveChangesAsync();

        return new UserResponseDto(
            newUser.Id, newUser.UserCode, newUser.FullName, newUser.Email, newUser.Role,
            newUser.AvatarUrl, newUser.DateOfBirth, newUser.AdministrativeClass, newUser.IsActive, newUser.CreatedAt
        );
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return false;

        // Chỉ cập nhật những trường được phép
        user.FullName = request.FullName;
        user.AvatarUrl = request.AvatarUrl;
        user.DateOfBirth = request.DateOfBirth;
        user.AdministrativeClass = request.AdministrativeClass;
        user.IsActive = request.IsActive;

        userRepository.Update(user);
        return await userRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return false;

        try
        {
            userRepository.Delete(user);
            return await userRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // PostgreSQL sẽ văng lỗi khi cố xóa một Record có dính Foreign Key
            throw new InvalidOperationException("Không thể xóa do tài khoản đang phát sinh dữ liệu, gợi ý: Hãy Khóa tài khoản thay vì Xóa.");
        }
    }

    public async Task<bool> ToggleUserStatusAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        userRepository.Update(user);

        return await userRepository.SaveChangesAsync();
    }
}