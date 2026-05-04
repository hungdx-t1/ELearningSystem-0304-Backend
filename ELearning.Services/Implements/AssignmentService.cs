using ELearning.Core.DTOs.Assignment;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;

namespace ELearning.Services.Implements;

public class AssignmentService(IGenericRepository<Assignment> assignmentRepository) : IAssignmentService
{
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

        await assignmentRepository.AddAsync(newAssignment);
        await assignmentRepository.SaveChangesAsync();

        return new AssignmentResponseDto(
            newAssignment.Id, newAssignment.LessonId, newAssignment.Title, newAssignment.Description, newAssignment.DueDate);
    }

    public async Task<bool> DeleteAssignmentAsync(Guid id)
    {
        var assignment = await assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
            return false;

        assignmentRepository.Delete(assignment);
        return await assignmentRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetAllAssignmentsAsync()
    {
        var assignments = await assignmentRepository.GetAllAsync();

        return assignments.Select(a => new AssignmentResponseDto(
            a.Id, a.LessonId, a.Title, a.Description, a.DueDate));
    }

    public async Task<AssignmentResponseDto?> GetAssignmentByIdAsync(Guid id)
    {
        var assignment = await assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
            return null;

        return new AssignmentResponseDto(
            assignment.Id, assignment.LessonId, assignment.Title, assignment.Description, assignment.DueDate);
    }

    public async Task<bool> UpdateAssignmentAsync(Guid id, UpdateAssignmentRequestDto request)
    {
        var assignment = await assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
            return false;

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.DueDate = request.DueDate;

        assignmentRepository.Update(assignment);
        return await assignmentRepository.SaveChangesAsync();
    }
}