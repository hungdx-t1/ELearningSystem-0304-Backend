namespace ELearning.Core.DTOs.Question;

public record QuestionResponseDto(
    Guid Id, 
    Guid LessonId, 
    string Content, 
    string OptionA, 
    string OptionB, 
    string OptionC, 
    string OptionD, 
    string CorrectOption, 
    string? Explanation
);

public record CreateQuestionRequestDto(
    Guid LessonId, 
    string Content, 
    string OptionA, 
    string OptionB, 
    string OptionC, 
    string OptionD, 
    string CorrectOption, 
    string? Explanation
);

public record UpdateQuestionRequestDto(
    string Content, 
    string OptionA, 
    string OptionB, 
    string OptionC, 
    string OptionD, 
    string CorrectOption, 
    string? Explanation
);