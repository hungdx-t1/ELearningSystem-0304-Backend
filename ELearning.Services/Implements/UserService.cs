using ELearning.Core.DTOs;
using ELearning.Core.DTOs.User;
using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

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

    public async Task<(int SuccessCount, List<string> Errors)> ImportUsersFromExcelAsync(Stream excelStream)
    {
        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");

        var successCount = 0;
        var errorRows = new List<string>();

        using (var package = new ExcelPackage(excelStream))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                var fullName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                var email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                var password = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                var roleString = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                var adminClass = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
                    continue;

                try
                {
                    UserRole role = UserRole.Student;
                    if (!string.IsNullOrEmpty(roleString))
                    {
                        var r = roleString.ToLower();
                        if (r.Contains("admin") || r.Contains("quản trị")) role = UserRole.Admin;
                        else if (r.Contains("instructor") || r.Contains("giảng viên")) role = UserRole.Instructor;
                    }

                    string prefix = role == UserRole.Student ? "STU" : (role == UserRole.Instructor ? "INS" : "ADM");
                    string randomSuffix = Guid.NewGuid().ToString()[..4].ToUpper();
                    string userCode = $"{prefix}-{DateTime.Now:yyMM}-{randomSuffix}";

                    string finalPassword = string.IsNullOrEmpty(password) ? "Default@123" : password;

                    var requestDto = new CreateUserRequestDto(
                        userCode,
                        fullName,
                        email,
                        finalPassword,
                        role,
                        adminClass
                    );

                    await CreateUserAsync(requestDto);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errorRows.Add($"Dòng {row} ({email}): {ex.Message}");
                }
            }
        }

        return (successCount, errorRows);
    }

    public async Task<byte[]> ExportUsersToExcelAsync()
    {
        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");

        var users = await GetAllUsersAsync();
        var userList = users.ToList();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("DanhSachNguoiDung");

        string[] headers = ["STT", "Mã Định Danh", "Họ và Tên", "Email", "Vai trò", "Lớp hành chính", "Trạng thái", "Ngày tham gia"];

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
        }

        using (var range = worksheet.Cells[1, 1, 1, headers.Length])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        for (int i = 0; i < userList.Count; i++)
        {
            var user = userList[i];
            var row = i + 2;

            string roleName = user.Role switch
            {
                UserRole.Admin => "Quản trị viên",
                UserRole.Instructor => "Giảng viên",
                UserRole.Student => "Học viên",
                _ => "Chưa xác định"
            };

            worksheet.Cells[row, 1].Value = i + 1;
            worksheet.Cells[row, 2].Value = user.UserCode;
            worksheet.Cells[row, 3].Value = user.FullName;
            worksheet.Cells[row, 4].Value = user.Email;
            worksheet.Cells[row, 5].Value = roleName;
            worksheet.Cells[row, 6].Value = user.AdministrativeClass ?? "Chưa xếp lớp";
            worksheet.Cells[row, 7].Value = user.IsActive ? "Hoạt động" : "Khóa";
            worksheet.Cells[row, 8].Value = user.CreatedAt.ToString("dd/MM/yyyy HH:mm");
        }

        worksheet.Cells.AutoFitColumns();

        return await package.GetAsByteArrayAsync();
    }
}