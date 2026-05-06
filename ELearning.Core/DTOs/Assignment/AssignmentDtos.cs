namespace ELearning.Core.DTOs.Assignment;

public record AssignmentResponseDto(
    Guid Id,
    Guid LessonId,
    string Title,
    string? Description,
    DateTime? DueDate
//DateTime CreatedAt
);

public record CreateAssignmentRequestDto(
    string Title,
    string? Description,
    DateTime? DueDate
);

public record UpdateAssignmentRequestDto(
    string Title,
    string? Description,
    DateTime? DueDate
);