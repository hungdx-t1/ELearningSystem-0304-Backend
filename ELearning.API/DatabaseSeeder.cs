using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Infrastructure.Data;
using BCrypt.Net;

namespace ELearning.API;

public static class DatabaseSeeder
{
    public static async Task SeedMockUsersAsync(AppDbContext context)
    {
        if (!context.Users.Any(u => u.Email == "admin@test.com" || u.UserCode == "ADM-0001"))
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),
                UserCode = "ADM-0001",
                FullName = "Quản Trị Viên (Test)",
                Email = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);
        }

        if (!context.Users.Any(u => u.Email == "instructor@test.com" || u.UserCode == "INS-0001"))
        {
            var instructor = new User
            {
                Id = Guid.NewGuid(),
                UserCode = "INS-0001",
                FullName = "Giảng Viên (Test)",
                Email = "instructor@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.Instructor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(instructor);
        }

        if (!context.Users.Any(u => u.Email == "student@test.com" || u.UserCode == "STU-0001"))
        {
            var student = new User
            {
                Id = Guid.NewGuid(),
                UserCode = "STU-0001",
                FullName = "Học Viên (Test)",
                Email = "student@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = UserRole.Student,
                AdministrativeClass = "Lớp Mẫu A",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(student);
        }

        // Lưu xuống DB nếu có đối tượng mới được thêm vào (Nghĩa là nếu 3 accounts trên chưa tồn tại)
        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync();
        }
    }
}