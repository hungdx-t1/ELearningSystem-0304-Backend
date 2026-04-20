using ELearning.Core.DTOs.Submission;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionsController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    // Đổi route để nhận cả ClassId và LessonId (Ví dụ: GET /api/submissions/class/123/lesson/456)
    [HttpGet("class/{classId:guid}/lesson/{lessonId:guid}")]
    public async Task<IActionResult> GetSubmissions(Guid classId, Guid lessonId)
    {
        return Ok(await _submissionService.GetSubmissionsAsync(classId, lessonId));
    }

    // API Sinh viên lấy lại bài nộp của chính mình
    [HttpGet("class/{classId:guid}/lesson/{lessonId:guid}/student/{studentId:guid}")]
    public async Task<IActionResult> GetStudentSubmission(Guid classId, Guid lessonId, Guid studentId)
    {
        var submission = await _submissionService.GetSubmissionAsync(classId, lessonId, studentId);
        if (submission == null) return NoContent();
        return Ok(submission);
    }

    [HttpPost("class/{classId:guid}/lesson/{lessonId:guid}/start-exam")]
    public async Task<IActionResult> StartExam(Guid classId, Guid lessonId)
    {
        // Lấy ID thật của sinh viên bằng claim Auth (Ví dụ mock 1 studentId từ route/auth nếu có, ở đây lấy user login)
        Guid studentId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _submissionService.StartExamAsync(classId, lessonId, studentId);
        return Ok(result);
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

    [HttpPost("submit-quiz")]
    public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequestDto request)
    {
        var result = await _submissionService.SubmitQuizAsync(request);
        return Ok(result);
    }

    [HttpGet("lesson/{lessonId:guid}/export")]
    public async Task<IActionResult> ExportScores(Guid lessonId)
    {
        var fileBytes = await _submissionService.ExportScoresToExcelAsync(lessonId);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bang_Diem.xlsx");
    }
}