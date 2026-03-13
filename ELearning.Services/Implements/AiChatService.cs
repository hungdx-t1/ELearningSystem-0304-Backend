using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Core.DTOs.AiChat;

namespace ELearning.Services.Implements;
public class AiChatService : IAiChatService
{
    private readonly IGenericRepository<AiChatLog> _logRepo;
    public AiChatService(IGenericRepository<AiChatLog> logRepo) => _logRepo = logRepo;

    public async Task<IEnumerable<AiChatLogDto>> GetUserChatHistoryAsync(Guid userId)
    {
        var logs = await _logRepo.FindAsync(l => l.UserId == userId);
        return logs.OrderBy(l => l.Timestamp).Select(l => new AiChatLogDto(l.Id, l.UserId, l.Message, l.Response, l.Timestamp));
    }

    public async Task LogChatAsync(CreateAiChatLogDto request)
    {
        var log = new AiChatLog
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Message = request.Message,
            Response = request.Response,
            Timestamp = DateTime.UtcNow
        };
        await _logRepo.AddAsync(log);
        await _logRepo.SaveChangesAsync();
    }
}