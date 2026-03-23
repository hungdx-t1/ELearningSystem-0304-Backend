using ELearning.Core.DTOs.Question;

namespace ELearning.Core.Interfaces.Services;

public interface IQuestionService
{
    Task<IEnumerable<QuestionResponseDto>> GetQuestionsByLessonIdAsync(Guid lessonId);
    Task<QuestionResponseDto> CreateQuestionAsync(CreateQuestionRequestDto request);
    Task<bool> UpdateQuestionAsync(Guid id, UpdateQuestionRequestDto request);
    Task<bool> DeleteQuestionAsync(Guid id);
}