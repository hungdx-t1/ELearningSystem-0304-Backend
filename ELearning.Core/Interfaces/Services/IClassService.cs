using ELearning.Core.DTOs.Class;

namespace ELearning.Core.Interfaces.Services;

public interface IClassService
{
    Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync();
    Task<ClassResponseDto> CreateClassAsync(CreateClassRequestDto request);
    Task<bool> EnrollStudentAsync(Guid classId, Guid studentId);
    Task<bool> UpdateClassAsync(Guid id, UpdateClassRequestDto request);
    Task<bool> DeleteClassAsync(Guid id);
}