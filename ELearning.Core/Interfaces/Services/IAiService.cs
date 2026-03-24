namespace ELearning.Core.Interfaces.Services;

public interface IAiService
{
    Task<string> ChatWithAiAsync(string userMessage);
    Task<string> GenerateQuizAsync(string topic, int questionCount);
}