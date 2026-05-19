using ELearning.Core.DTOs.Lesson;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LessonsController(ILessonService lessonService) : ControllerBase
{
    [HttpGet("chapter/{chapterId:guid}")]
    [EndpointSummary("Lấy danh sách bài học của chương")]
    [EndpointDescription("Truy xuất danh sách tất cả các bài học trong một chương cụ thể.")]
    public async Task<ActionResult<IEnumerable<LessonResponseDto>>> GetLessonsByChapter(Guid chapterId)
    {
        var lessons = await lessonService.GetLessonsByChapterIdAsync(chapterId);
        return Ok(lessons);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Lấy chi tiết bài học bằng ID")]
    [EndpointDescription("Truy xuất thông tin chi tiết của một bài học cụ thể thông qua ID.")]
    public async Task<ActionResult<LessonResponseDto>> GetById(Guid id)
    {
        var lesson = await lessonService.GetLessonByIdAsync(id);
        if (lesson == null) return NotFound();
        return Ok(lesson);
    }

    // Tạo bài học mới (Video Youtube, PDF, v.v.)
    [HttpPost]
    [EndpointSummary("Tạo mới bài học")]
    [EndpointDescription("Tạo một bài học mới trong hệ thống.")]
    public async Task<ActionResult<LessonResponseDto>> Create([FromBody] CreateLessonRequestDto request)
    {
        try
        {
            var newLesson = await lessonService.CreateLessonAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = newLesson.Id }, newLesson);
        }
        catch (ArgumentException ex) // bắt lỗi từ service nếu có
        {
            // Trả về mã 400 Bad Request kèm JSON để Angular đọc được
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Cập nhật bài học")]
    [EndpointDescription("Cập nhật thông tin của một bài học cụ thể.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonRequestDto request)
    {
        try
        {
            var isUpdated = await lessonService.UpdateLessonAsync(id, request);
            if (!isUpdated) return NotFound(new { message = "Không tìm thấy bài học" });
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            // Trả về mã 400 Bad Request kèm JSON
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Xóa bài học")]
    [EndpointDescription("Xóa vĩnh viễn hoặc khóa một bài học khỏi hệ thống.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await lessonService.DeleteLessonAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy bài học" });
        return NoContent();
    }

    [HttpPut("update-order")]
    [EndpointSummary("Cập nhật thứ tự bài học")]
    [EndpointDescription("Cập nhật thứ tự của các bài học trong một chương.")]
    public async Task<IActionResult> UpdateOrder([FromBody] IEnumerable<UpdateLessonOrderDto> request)
    {
        var result = await lessonService.UpdateLessonOrdersAsync(request);
        if (!result) return BadRequest(new { message = "Lỗi khi cập nhật thứ tự bài học." });
        return NoContent();
    }
}