using ELearning.Core.DTOs.Class;

namespace ELearning.Core.Interfaces.Services;

public interface IClassLessonScheduleService
{
    Task<ClassLessonScheduleResponseDto?> GetScheduleAsync(Guid classId, Guid lessonId);
    Task<ClassLessonScheduleResponseDto> UpsertScheduleAsync(Guid classId, Guid lessonId, UpsertClassLessonScheduleRequestDto request);
}
