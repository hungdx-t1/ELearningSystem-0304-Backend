using ELearning.Core.DTOs.Class;
using Microsoft.AspNetCore.Http;

namespace ELearning.Core.Interfaces.Services;

public interface IClassService
{
    Task<bool> IsClassOwnerOrAdminAsync(Guid classId, Guid userId, string role);
    Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync(Guid? instructorId = null);
    Task<ClassResponseDto> CreateClassAsync(CreateClassRequestDto request);
    Task<bool> EnrollStudentAsync(Guid classId, Guid studentId);
    Task<bool> EnrollStudentByEmailAsync(Guid classId, string emailOrCode);
    Task<bool> UpdateClassAsync(Guid id, UpdateClassRequestDto request);
    Task<bool> DeleteClassAsync(Guid id);
    Task<ClassDetailsResponseDto?> GetClassDetailsAsync(Guid classId, Guid currentUserId, string currentUserRole);
    Task<IEnumerable<StudentClassResponseDto>> GetClassesByStudentAsync(Guid studentId);
    Task<(int AddedCount, List<string> Errors)> ImportStudentsFromExcelAsync(Guid classId, IFormFile file);
    Task<bool> RemoveStudentFromClassAsync(Guid classId, Guid studentId);
}