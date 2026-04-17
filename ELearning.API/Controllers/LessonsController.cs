using ELearning.Core.DTOs.Lesson;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        try
        {
            var newLesson = await _lessonService.CreateLessonAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = newLesson.Id }, newLesson);
        }
        catch (ArgumentException ex) // bắt lỗi từ service nếu có
        {
            // Trả về mã 400 Bad Request kèm JSON để Angular đọc được
            return BadRequest(new { message = ex.Message }); 
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonRequestDto request)
    {
        try
        {
            var isUpdated = await _lessonService.UpdateLessonAsync(id, request);
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
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _lessonService.DeleteLessonAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy bài học" });
        return NoContent();
    }

    [HttpPut("update-order")]
    public async Task<IActionResult> UpdateOrder([FromBody] IEnumerable<UpdateLessonOrderDto> request)
    {
        var result = await _lessonService.UpdateLessonOrdersAsync(request);
        if (!result) return BadRequest(new { message = "Lỗi khi cập nhật thứ tự bài học." });
        return NoContent();
    }
}