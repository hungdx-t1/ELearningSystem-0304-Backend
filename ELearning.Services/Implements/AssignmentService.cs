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

    public async Task<AssignmentResponseDto> CreateAssignmentAsync(CreateAssignmentRequestDto request)
    {
        var newAssignment = new Assignment
        {
            Id = Guid.NewGuid(),
            LessonId = Guid.Empty, // TODO: Cần có cách xác định LessonId khi tạo bài tập
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate
        };

        await _assignmentRepository.AddAsync(newAssignment);
        await _assignmentRepository.SaveChangesAsync();
        
        return new AssignmentResponseDto(
            newAssignment.Id, newAssignment.LessonId, newAssignment.Title, newAssignment.Description, newAssignment.DueDate);
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
            return false;

        _assignmentRepository.Delete(assignment);
        return await _assignmentRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetAllAssignmentsAsync()
    {
        var assignments = await _assignmentRepository.GetAllAsync();
        
        return assignments.Select(a => new AssignmentResponseDto(
            a.Id, a.LessonId, a.Title, a.Description, a.DueDate));
    }

    public async Task<AssignmentResponseDto?> GetAssignmentByIdAsync(Guid id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
            return null;

        return new AssignmentResponseDto(
            assignment.Id, assignment.LessonId, assignment.Title, assignment.Description, assignment.DueDate);
    }

    public async Task<bool> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequestDto request)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
            return false;

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.DueDate = request.DueDate;

        _assignmentRepository.Update(assignment);
        return await _assignmentRepository.SaveChangesAsync();
    }
}