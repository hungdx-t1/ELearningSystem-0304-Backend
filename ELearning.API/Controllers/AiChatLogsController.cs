using ELearning.Core.DTOs.AiChat;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiChatLogsController : ControllerBase
{
    private readonly IAiChatService _aiChatService;

    public AiChatLogsController(IAiChatService aiChatService) => _aiChatService = aiChatService;

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetHistory(Guid userId)
    {
        return Ok(await _aiChatService.GetUserChatHistoryAsync(userId));
    }

    [HttpPost]
    public async Task<IActionResult> SaveLog([FromBody] CreateAiChatLogDto request)
    {
        await _aiChatService.LogChatAsync(request);
        return Ok(new { message = "Đã lưu lịch sử chat." });
    }
}