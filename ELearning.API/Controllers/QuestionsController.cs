using ELearning.Core.DTOs.Question;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    // Lấy toàn bộ câu hỏi của 1 bài học
    [HttpGet("lesson/{lessonId:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy danh sách câu hỏi của bài học")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất danh sách tất cả các câu hỏi trong một bài học cụ thể.")]
    public async Task<IActionResult> GetByLesson(Guid lessonId)
    {
        return Ok(await _questionService.GetQuestionsByLessonIdAsync(lessonId));
    }

    [HttpPost]
    [Microsoft.AspNetCore.Http.EndpointSummary("Tạo mới câu hỏi")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Tạo một câu hỏi mới trong hệ thống.")]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequestDto request)
    {
        var question = await _questionService.CreateQuestionAsync(request);
        return Ok(question);
    }

    [HttpPut("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Cập nhật câu hỏi")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Cập nhật thông tin của một câu hỏi cụ thể.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionRequestDto request)
    {
        var isUpdated = await _questionService.UpdateQuestionAsync(id, request);
        if (!isUpdated) return NotFound("Không tìm thấy câu hỏi");
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Xóa câu hỏi")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Xóa vĩnh viễn hoặc khóa một câu hỏi khỏi hệ thống.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _questionService.DeleteQuestionAsync(id);
        if (!isDeleted) return NotFound("Không tìm thấy câu hỏi");
        return NoContent();
    }
}