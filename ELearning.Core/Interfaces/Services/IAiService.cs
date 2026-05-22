using Microsoft.AspNetCore.Http;

namespace ELearning.Core.Interfaces.Services;

public interface IAiService
{
    Task<Pgvector.Vector> GenerateEmbeddingAsync(string text);
    Task<string> ChatWithAiAsync(string userMessage, List<Guid>? lessonIds, IFormFile? file = null, string? similarContext = null, string? userName = null);
    Task<string> GenerateQuizAsync(string topic, int questionCount, List<Guid>? lessonIds, IFormFile? file = null);
}