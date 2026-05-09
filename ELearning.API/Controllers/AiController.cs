using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController(IAiService aiService) : ControllerBase
{
    public record GenerateQuizRequest(string Topic, int QuestionCount);

    // Dữ liệu client gửi lên
    public record ChatRequest(string Prompt);

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromForm] string prompt, [FromForm] IFormFile? file)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest(new { message = "Câu hỏi không được để trống" });

        var reply = await aiService.ChatWithAiAsync(prompt, file);

        return Ok(new { reply });
    }

    [HttpPost("generate-quiz")]
    public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Topic) || request.QuestionCount <= 0 || request.QuestionCount > 20)
            return BadRequest(new { message = "Vui lòng nhập chủ đề và số lượng câu hỏi hợp lệ (1-20)." });

        var jsonResult = await aiService.GenerateQuizAsync(request.Topic, request.QuestionCount);

        try
        {
            // Vì AI trả về một chuỗi JSON thuần, ta Parse nó thành Object để trả về cho Frontend dưới dạng mảng đàng hoàng
            var jsonElement = System.Text.Json.JsonSerializer.Deserialize<object>(jsonResult);
            return Ok(jsonElement);
        }
        catch
        {
            // Đề phòng trường hợp AI bị ngáo trả về text linh tinh không parse được
            return StatusCode(500, new { message = "Lỗi khi xử lý dữ liệu từ AI." });
        }
    }

    [HttpPost("generate-quiz-from-file")]
    public async Task<IActionResult> GenerateQuizFromFile([FromForm] IFormFile file, [FromForm] string? topic, [FromForm] int questionCount)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng đính kèm tài liệu." });

        if (questionCount <= 0 || questionCount > 20)
            return BadRequest(new { message = "Số lượng câu hỏi phải từ 1 đến 20." });

        // Nếu topic bị null thì cho thành chuỗi rỗng
        var safeTopic = topic ?? "";

        var jsonResult = await aiService.GenerateQuizFromFileAsync(file, safeTopic, questionCount);

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