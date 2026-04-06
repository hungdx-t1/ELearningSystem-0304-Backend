using Microsoft.AspNetCore.Http;

namespace ELearning.Core.Interfaces.Services;

public interface IAiService
{
   // Task<string> ChatWithAiAsync(string userMessage);
    Task<string> GenerateQuizAsync(string topic, int questionCount);
    Task<string> GenerateQuizFromFileAsync(IFormFile file, string topic, int questionCount);
    Task<string> ChatWithAiAsync(string userMessage, IFormFile? file = null);
}