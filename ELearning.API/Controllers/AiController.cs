using ELearning.Core.Interfaces;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController(IAiService aiService, IAiChatService aiChatService, IGenericRepository<User> userRepo) : ControllerBase
{
    [HttpPost("chat")]
    [EndpointSummary("Tương tác AI Chat")]
    [EndpointDescription("Giao tiếp và nhận phản hồi từ trợ lý ảo AI (Hỗ trợ text, up file, và chọn bài học).")]
    public async Task<IActionResult> Chat([FromForm] string prompt, [FromForm] List<Guid>? lessonIds, [FromForm] IFormFile? file)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest(new { message = "Câu hỏi không được để trống" });

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? similarContext = null;
        string? userName = null;

        if (Guid.TryParse(userIdString, out var userId))
        {
            var user = await userRepo.GetByIdAsync(userId);
            userName = user?.FullName;

            var similarChats = await aiChatService.FindSimilarChatsAsync(userId, prompt, 3);
            if (similarChats.Any())
            {
                similarContext = string.Join("\n\n", similarChats.Select(c => $"Học viên: {c.Message}\nTrợ lý AI: {c.Response}"));
            }
        }

        var reply = await aiService.ChatWithAiAsync(prompt, lessonIds, file, similarContext, userName);
        return Ok(new { reply });
    }

    [HttpPost("generate-quiz")]
    [EndpointSummary("Tạo câu hỏi tự động bằng AI (Hỗ trợ Text, File, Bài học)")]
    [EndpointDescription("Sử dụng AI để sinh câu hỏi trắc nghiệm dựa trên chủ đề, tài liệu tải lên hoặc tài liệu có sẵn trong khóa học.")]
    public async Task<IActionResult> GenerateQuiz(
        [FromForm] string? topic,
        [FromForm] int questionCount,
        [FromForm] List<Guid>? lessonIds,
        [FromForm] IFormFile? file)
    {
        if (questionCount <= 0 || questionCount > 20)
            return BadRequest(new { message = "Số lượng câu hỏi phải từ 1 đến 20." });

        var safeTopic = topic ?? "tổng hợp kiến thức";

        var jsonResult = await aiService.GenerateQuizAsync(safeTopic, questionCount, lessonIds, file);

        try
        {
            var jsonElement = System.Text.Json.JsonSerializer.Deserialize<object>(jsonResult);
            return Ok(jsonElement);
        }
        catch
        {
            return StatusCode(500, new { message = "Lỗi khi xử lý dữ liệu từ AI." });
        }
    }
}