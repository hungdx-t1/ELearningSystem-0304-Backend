namespace ELearning.Core.DTOs.Ai;

public record ChatRequest(string Prompt, List<Guid>? LessonIds); // Dữ liệu Client gửi lên cho Chat
public record GenerateQuizRequest(string Topic, int QuestionCount, List<Guid>? LessonIds); // Dữ liệu Client gửi lên cho việc tạo Quiz