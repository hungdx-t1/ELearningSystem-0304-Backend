using ELearning.Core.DTOs.Chapter;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChaptersController(IChapterService chapterService) : ControllerBase
{
    // Lấy danh sách chương theo Khóa học
    [HttpGet("course/{courseId:guid}")]
    [EndpointSummary("Lấy danh sách chương theo khóa học")]
    [EndpointDescription("Truy xuất danh sách chương theo khóa học.")]
    public async Task<ActionResult<IEnumerable<ChapterResponseDto>>> GetChaptersByCourse(Guid courseId)
    {
        var chapters = await chapterService.GetChaptersByCourseIdAsync(courseId);
        return Ok(chapters);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Lấy chi tiết chương bằng ID")]
    [EndpointDescription("Truy xuất thông tin chi tiết của một chương thông qua ID.")]
    public async Task<ActionResult<ChapterResponseDto>> GetById(Guid id)
    {
        var chapter = await chapterService.GetChapterByIdAsync(id);
        if (chapter == null) return NotFound();
        return Ok(chapter);
    }

    [HttpPost]
    [EndpointSummary("Tạo mới một chương")]
    [EndpointDescription("Tạo một chương mới trong hệ thống.")]
    public async Task<ActionResult<ChapterResponseDto>> Create([FromBody] CreateChapterRequestDto request)
    {
        var newChapter = await chapterService.CreateChapterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newChapter.Id }, newChapter);
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Cập nhật một chương")]
    [EndpointDescription("Cập nhật thông tin của một chương đã tồn tại.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChapterRequestDto request)
    {
        var isUpdated = await chapterService.UpdateChapterAsync(id, request);
        if (!isUpdated) return NotFound(new { message = "Không tìm thấy chương" });
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Xóa một chương")]
    [EndpointDescription("Xóa một chương khỏi hệ thống.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await chapterService.DeleteChapterAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy chương" });
        return NoContent();
    }
}