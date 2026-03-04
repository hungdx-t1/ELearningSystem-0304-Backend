using ELearning.Core.DTOs.Lesson;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    // Lấy danh sách bài học theo Chapter (Chương)
    [HttpGet("chapter/{chapterId:guid}")]
    public async Task<ActionResult<IEnumerable<LessonResponseDto>>> GetLessonsByChapter(Guid chapterId)
    {
        // Trong ILessonService bạn cần viết thêm hàm GetByChapterIdAsync
        var lessons = await _lessonService.GetLessonsByChapterIdAsync(chapterId);
        return Ok(lessons);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LessonResponseDto>> GetById(Guid id)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(id);
        if (lesson == null) return NotFound();
        return Ok(lesson);
    }

    // Tạo bài học mới (Video Youtube, PDF, v.v.)
    [HttpPost]
    public async Task<ActionResult<LessonResponseDto>> Create([FromBody] CreateLessonRequestDto request)
    {
        var newLesson = await _lessonService.CreateLessonAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newLesson.Id }, newLesson);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonRequestDto request)
    {
        var isUpdated = await _lessonService.UpdateLessonAsync(id, request);
        if (!isUpdated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _lessonService.DeleteLessonAsync(id);
        if (!isDeleted) return NotFound();
        return NoContent();
    }
}