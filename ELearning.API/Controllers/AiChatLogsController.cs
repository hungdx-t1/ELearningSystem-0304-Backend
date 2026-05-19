using ELearning.Core.DTOs.AiChatLog;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiChatLogsController(IAiChatService aiChatService) : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    [EndpointSummary("Lấy chi tiết lịch sử tương tác AI bằng ID")]
    [EndpointDescription("Truy xuất lịch sử tương tác AI cụ thể thông qua ID.")]
    public async Task<IActionResult> GetHistory(Guid userId)
    {
        return Ok(await aiChatService.GetUserChatHistoryAsync(userId));
    }

    [HttpPost]
    [EndpointSummary("Lưu lịch sử tương tác AI")]
    [EndpointDescription("Endpoint API xử lý lưu lịch sử tương tác AI.")]
    public async Task<IActionResult> SaveLog([FromBody] CreateAiChatLogDto request)
    {
        await aiChatService.LogChatAsync(request);
        return Ok(new { message = "Đã lưu lịch sử chat." });
    }
}