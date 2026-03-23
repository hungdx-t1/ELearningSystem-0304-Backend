namespace ELearning.Core.Entities;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid LessonId { get; set; }
    public Lesson? Lesson { get; set; } // 1 bài học (Quiz) có nhiều câu hỏi

    public string Content { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    
    public string CorrectOption { get; set; } = "A"; // Lưu giá trị "A", "B", "C", hoặc "D"
    
    public string? Explanation { get; set; } // Lời giải thích tùy chọn
}