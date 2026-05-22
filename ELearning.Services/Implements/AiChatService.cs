using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Core.DTOs.AiChatLog;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace ELearning.Services.Implements;
public class AiChatService(IGenericRepository<AiChatLog> logRepo, IAiService aiService, AppDbContext context) : IAiChatService
{
    public async Task<IEnumerable<AiChatLogDto>> GetUserChatHistoryAsync(Guid userId)
    {
        var logs = await logRepo.FindAsync(l => l.UserId == userId);
        return logs.OrderBy(l => l.Timestamp).Select(l => new AiChatLogDto(l.Id, l.UserId, l.Message, l.Response, l.Timestamp));
    }

    public async Task<IEnumerable<AiChatLogDto>> FindSimilarChatsAsync(Guid userId, string prompt, int limit = 3)
    {
        var promptVector = await aiService.GenerateEmbeddingAsync(prompt);
        
        var similarLogs = await context.AiChatLogs
            .Where(l => l.UserId == userId && l.Embedding != null)
            .OrderBy(l => l.Embedding!.CosineDistance(promptVector))
            .Take(limit)
            .ToListAsync();

        return similarLogs.Select(l => new AiChatLogDto(l.Id, l.UserId, l.Message, l.Response, l.Timestamp));
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

        try
        {
            log.Embedding = await aiService.GenerateEmbeddingAsync($"User: {request.Message}\nAI: {request.Response}");
        }
        catch
        {
            // Fallback nếu lỗi sinh vector
            log.Embedding = null;
        }

        await logRepo.AddAsync(log);
        await logRepo.SaveChangesAsync();
    }
}