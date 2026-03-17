using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // tạm thời comment để dễ test, sau này sẽ thêm lại
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

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
}