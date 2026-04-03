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
    public async Task<IActionResult> GetByLesson(Guid lessonId)
    {
        return Ok(await _questionService.GetQuestionsByLessonIdAsync(lessonId));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequestDto request)
    {
        var question = await _questionService.CreateQuestionAsync(request);
        return Ok(question);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionRequestDto request)
    {
        var isUpdated = await _questionService.UpdateQuestionAsync(id, request);
        if (!isUpdated) return NotFound("Không tìm thấy câu hỏi");
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _questionService.DeleteQuestionAsync(id);
        if (!isDeleted) return NotFound("Không tìm thấy câu hỏi");
        return NoContent();
    }
}