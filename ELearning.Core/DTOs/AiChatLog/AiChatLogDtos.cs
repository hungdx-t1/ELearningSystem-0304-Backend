namespace ELearning.Core.DTOs.AiChat;
public record AiChatLogDto(Guid Id, Guid? UserId, string Message, string Response, DateTime Timestamp);
public record CreateAiChatLogDto(Guid? UserId, string Message, string Response);