using ELearning.Core.DTOs.Assignment;

namespace ELearning.Core.Interfaces.Services;

public interface IAssignmentService
{
    Task<IEnumerable<AssignmentResponseDto>> GetAllAssignmentsAsync();
    Task<AssignmentResponseDto?> GetAssignmentByIdAsync(Guid id);
    Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentRequestDto request);
    Task<bool> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequestDto request);
    Task<bool> DeleteAssignmentAsync(Guid id);
}