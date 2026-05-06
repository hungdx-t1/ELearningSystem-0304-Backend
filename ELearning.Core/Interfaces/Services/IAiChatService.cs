using ELearning.Core.DTOs.AiChatLog;
namespace ELearning.Core.Interfaces.Services;
public interface IAiChatService
{
    Task<IEnumerable<AiChatLogDto>> GetUserChatHistoryAsync(Guid userId);
    Task LogChatAsync(CreateAiChatLogDto request);
}