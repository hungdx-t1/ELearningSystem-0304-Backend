using ELearning.Core.DTOs.Submission;

namespace ELearning.Core.Interfaces.Services;

public interface ISubmissionService
{
    Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsByAssignmentIdAsync(Guid assignmentId);
    Task<SubmissionResponseDto?> GetSubmissionAsync(Guid assignmentId, Guid studentId);
    Task<SubmissionResponseDto> SubmitWorkAsync(CreateSubmissionRequestDto request);
    Task<bool> GradeSubmissionAsync(Guid id, GradeSubmissionRequestDto request);
}