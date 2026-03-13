using ELearning.Core.DTOs.Submission;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionsController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpGet("assignment/{assignmentId:guid}")]
    public async Task<IActionResult> GetByAssignment(Guid assignmentId)
    {
        return Ok(await _submissionService.GetSubmissionsByAssignmentIdAsync(assignmentId));
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitWork([FromBody] CreateSubmissionRequestDto request)
    {
        var result = await _submissionService.SubmitWorkAsync(request);
        return Ok(result);
    }

    [HttpPut("{id:guid}/grade")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeSubmissionRequestDto request)
    {
        var isGraded = await _submissionService.GradeSubmissionAsync(id, request);
        if (!isGraded) return NotFound("Không tìm thấy bài nộp.");
        return NoContent();
    }
}