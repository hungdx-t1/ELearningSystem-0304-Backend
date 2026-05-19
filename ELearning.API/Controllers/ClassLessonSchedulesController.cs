using ELearning.Core.DTOs.Class;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassLessonSchedulesController(IClassLessonScheduleService scheduleService) : ControllerBase
{
    [HttpGet("class/{classId:guid}/lesson/{lessonId:guid}")]
    [EndpointSummary("Lấy lịch trình của một bài học")]
    [EndpointDescription("Trả về chi tiết về lịch học (thời gian bắt đầu, kết thúc) của một bài học cụ thể trong một lớp học.")]
    public async Task<IActionResult> GetSchedule(Guid classId, Guid lessonId)
    {
        var schedule = await scheduleService.GetScheduleAsync(classId, lessonId);
        if (schedule == null) return NoContent();
        return Ok(schedule);
    }

    [HttpPost("class/{classId:guid}/lesson/{lessonId:guid}")]
    [EndpointSummary("Tạo mới hoặc cập nhật lịch trình")]
    [EndpointDescription("Dùng để thiết lập lịch mở/đóng bài học. Nếu bài học chưa có lịch trong lớp này, nó sẽ được tạo mới. Nếu đã có, nó sẽ được cập nhật.")]
    public async Task<IActionResult> UpsertSchedule(Guid classId, Guid lessonId, [FromBody] UpsertClassLessonScheduleRequestDto request)
    {
        var result = await scheduleService.UpsertScheduleAsync(classId, lessonId, request);
        return Ok(result);
    }
}
