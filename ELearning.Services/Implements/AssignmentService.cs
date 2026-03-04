using ELearning.Core.DTOs.Assignment;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;

namespace ELearning.Services.Implements;

public class AssignmentService : IAssignmentService
{
    private readonly IGenericRepository<Assignment> _assignmentRepository;

    public AssignmentService(IGenericRepository<Assignment> assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetAllAssignmentsAsync()
    {
        var assignments = await _assignmentRepository.GetAllAsync();
        
        return assignments.Select(a => new AssignmentResponseDto(
            a.Id, a.LessonId, a.Title, a.Description, a.DueDate, a.CreatedAt));
    }

}