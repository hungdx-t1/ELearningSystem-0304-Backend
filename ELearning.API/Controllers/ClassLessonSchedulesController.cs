using ELearning.Core.DTOs.Class;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassLessonSchedulesController : ControllerBase
{
    private readonly IClassLessonScheduleService _scheduleService;

    public ClassLessonSchedulesController(IClassLessonScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [HttpGet("class/{classId:guid}/lesson/{lessonId:guid}")]
    public async Task<IActionResult> GetSchedule(Guid classId, Guid lessonId)
    {
        var schedule = await _scheduleService.GetScheduleAsync(classId, lessonId);
        if (schedule == null) return NoContent();
        return Ok(schedule);
    }

    [HttpPost("class/{classId:guid}/lesson/{lessonId:guid}")]
    public async Task<IActionResult> UpsertSchedule(Guid classId, Guid lessonId, [FromBody] UpsertClassLessonScheduleRequestDto request)
    {
        var result = await _scheduleService.UpsertScheduleAsync(classId, lessonId, request);
        return Ok(result);
    }
}
