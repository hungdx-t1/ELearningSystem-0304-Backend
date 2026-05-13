using Microsoft.AspNetCore.Http;

namespace ELearning.Core.Interfaces.Services;

public interface IAiService
{
    Task<string> ChatWithAiAsync(string userMessage, List<Guid>? lessonIds, IFormFile? file = null);
    Task<string> GenerateQuizAsync(string topic, int questionCount, List<Guid>? lessonIds, IFormFile? file = null);
}