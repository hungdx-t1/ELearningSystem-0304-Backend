using ELearning.Core.DTOs.Question;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;

namespace ELearning.Services.Implements;

public class QuestionService(IGenericRepository<Question> questionRepo) : IQuestionService
{
    public async Task<IEnumerable<QuestionResponseDto>> GetQuestionsByLessonIdAsync(Guid lessonId)
    {
        var questions = await questionRepo.FindAsync(q => q.LessonId == lessonId);
        return questions.Select(q => new QuestionResponseDto(
            q.Id, q.LessonId, q.Content, q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.CorrectOption, q.Explanation
        ));
    }

    public async Task<QuestionResponseDto> CreateQuestionAsync(CreateQuestionRequestDto request)
    {
        var question = new Question
        {
            LessonId = request.LessonId,
            Content = request.Content,
            OptionA = request.OptionA,
            OptionB = request.OptionB,
            OptionC = request.OptionC,
            OptionD = request.OptionD,
            CorrectOption = request.CorrectOption,
            Explanation = request.Explanation
        };

        await questionRepo.AddAsync(question);
        await questionRepo.SaveChangesAsync();

        return new QuestionResponseDto(
            question.Id, question.LessonId, question.Content, question.OptionA, question.OptionB, question.OptionC, question.OptionD, question.CorrectOption, question.Explanation
        );
    }

    public async Task<bool> UpdateQuestionAsync(Guid id, UpdateQuestionRequestDto request)
    {
        var question = await questionRepo.GetByIdAsync(id);
        if (question == null) return false;

        question.Content = request.Content;
        question.OptionA = request.OptionA;
        question.OptionB = request.OptionB;
        question.OptionC = request.OptionC;
        question.OptionD = request.OptionD;
        question.CorrectOption = request.CorrectOption;
        question.Explanation = request.Explanation;

        questionRepo.Update(question);
        return await questionRepo.SaveChangesAsync();
    }

    public async Task<bool> DeleteQuestionAsync(Guid id)
    {
        var question = await questionRepo.GetByIdAsync(id);
        if (question == null) return false;

        questionRepo.Delete(question);
        return await questionRepo.SaveChangesAsync();
    }
}