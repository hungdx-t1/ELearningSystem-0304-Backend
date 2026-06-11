using ELearning.Core.DTOs.AiChatLog;
namespace ELearning.Core.Interfaces.Services;
public interface IAiChatService
{
    Task<IEnumerable<AiChatLogDto>> GetUserChatHistoryAsync(Guid userId);
    Task<IEnumerable<AiChatLogDto>> GetRecentChatsAsync(Guid userId, int limit = 6);
    Task<IEnumerable<AiChatLogDto>> FindSimilarChatsAsync(Guid userId, string prompt, int limit = 3);
    Task LogChatAsync(CreateAiChatLogDto request);
}