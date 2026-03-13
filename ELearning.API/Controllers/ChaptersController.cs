using ELearning.Core.DTOs.Chapter;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChaptersController : ControllerBase
{
    private readonly IChapterService _chapterService;

    public ChaptersController(IChapterService chapterService)
    {
        _chapterService = chapterService;
    }

    // Lấy danh sách chương theo Khóa học
    [HttpGet("course/{courseId:guid}")]
    public async Task<ActionResult<IEnumerable<ChapterResponseDto>>> GetChaptersByCourse(Guid courseId)
    {
        var chapters = await _chapterService.GetChaptersByCourseIdAsync(courseId);
        return Ok(chapters);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ChapterResponseDto>> GetById(Guid id)
    {
        var chapter = await _chapterService.GetChapterByIdAsync(id);
        if (chapter == null) return NotFound();
        return Ok(chapter);
    }

    [HttpPost]
    public async Task<ActionResult<ChapterResponseDto>> Create([FromBody] CreateChapterRequestDto request)
    {
        var newChapter = await _chapterService.CreateChapterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newChapter.Id }, newChapter);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChapterRequestDto request)
    {
        var isUpdated = await _chapterService.UpdateChapterAsync(id, request);
        if (!isUpdated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _chapterService.DeleteChapterAsync(id);
        if (!isDeleted) return NotFound();
        return NoContent();
    }
}