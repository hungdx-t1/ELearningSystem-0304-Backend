using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // tạm thời comment để dễ test, sau này sẽ thêm lại
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public record GenerateQuizRequest(string Topic, int QuestionCount);

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    // Dữ liệu client gửi lên
    public record ChatRequest(string Prompt);

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest(new { message = "Câu hỏi không được để trống" });

        var reply = await _aiService.ChatWithAiAsync(request.Prompt);
        
        // Trả về theo định dạng { reply: "..." } để khớp với Frontend
        return Ok(new { reply = reply });
    }

    [HttpPost("generate-quiz")]
    public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Topic) || request.QuestionCount <= 0 || request.QuestionCount > 20)
            return BadRequest(new { message = "Vui lòng nhập chủ đề và số lượng câu hỏi hợp lệ (1-20)." });

        var jsonResult = await _aiService.GenerateQuizAsync(request.Topic, request.QuestionCount);
        
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
}