namespace ELearning.Core.DTOs.Class;

public record ClassLessonScheduleResponseDto(
    Guid ClassId,
    Guid LessonId,
    DateTime? StartTime,
    DateTime? DueDate,
    int? OverrideDuration
);

public record UpsertClassLessonScheduleRequestDto(
    DateTime? StartTime,
    DateTime? DueDate,
    int? OverrideDuration
);
