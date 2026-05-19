using ELearning.Core.DTOs.Question;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    // Lấy toàn bộ câu hỏi của 1 bài học
    [HttpGet("lesson/{lessonId:guid}")]
    [EndpointSummary("Lấy danh sách câu hỏi của bài học")]
    [EndpointDescription("Truy xuất danh sách tất cả các câu hỏi trong một bài học cụ thể.")]
    public async Task<IActionResult> GetByLesson(Guid lessonId)
    {
        return Ok(await questionService.GetQuestionsByLessonIdAsync(lessonId));
    }

    [HttpPost]
    [EndpointSummary("Tạo mới câu hỏi")]
    [EndpointDescription("Tạo một câu hỏi mới trong hệ thống.")]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequestDto request)
    {
        var question = await questionService.CreateQuestionAsync(request);
        return Ok(question);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Cập nhật câu hỏi")]
    [EndpointDescription("Cập nhật thông tin của một câu hỏi cụ thể.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionRequestDto request)
    {
        var isUpdated = await questionService.UpdateQuestionAsync(id, request);
        if (!isUpdated) return NotFound("Không tìm thấy câu hỏi");
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Xóa câu hỏi")]
    [EndpointDescription("Xóa vĩnh viễn hoặc khóa một câu hỏi khỏi hệ thống.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await questionService.DeleteQuestionAsync(id);
        if (!isDeleted) return NotFound("Không tìm thấy câu hỏi");
        return NoContent();
    }
}