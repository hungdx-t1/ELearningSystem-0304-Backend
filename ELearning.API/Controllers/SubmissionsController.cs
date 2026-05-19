using ELearning.Core.DTOs.Submission;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController(ISubmissionService submissionService) : ControllerBase
{
    // Đổi route để nhận cả ClassId và LessonId (Ví dụ: GET /api/submissions/class/123/lesson/456)
    [HttpGet("class/{classId:guid}/lesson/{lessonId:guid}")]
    [EndpointSummary("Lấy danh sách bài nộp của bài học")]
    [EndpointDescription("Truy xuất danh sách tất cả các bài nộp của một bài học cụ thể.")]
    public async Task<IActionResult> GetSubmissions(Guid classId, Guid lessonId)
    {
        return Ok(await submissionService.GetSubmissionsAsync(classId, lessonId));
    }

    // API Sinh viên lấy lại bài nộp của chính mình
    [HttpGet("class/{classId:guid}/lesson/{lessonId:guid}/student/{studentId:guid}")]
    [EndpointSummary("Lấy chi tiết bài nộp bằng ID")]
    [EndpointDescription("Truy xuất thông tin chi tiết của một bài nộp cụ thể.")]
    public async Task<IActionResult> GetStudentSubmission(Guid classId, Guid lessonId, Guid studentId)
    {
        var submission = await submissionService.GetSubmissionAsync(classId, lessonId, studentId);
        if (submission == null) return NoContent();
        return Ok(submission);
    }

    [HttpGet("student/history")]
    [EndpointSummary("Lấy lịch sử bài nộp của sinh viên")]
    public async Task<IActionResult> GetStudentHistory()
    {
        Guid studentId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var history = await submissionService.GetStudentHistoryAsync(studentId);
        return Ok(history);
    }

    [HttpPost("class/{classId:guid}/lesson/{lessonId:guid}/start-exam")]
    [EndpointSummary("Bắt đầu bài kiểm tra")]
    [EndpointDescription("Học viên bắt đầu làm bài kiểm tra.")]
    public async Task<IActionResult> StartExam(Guid classId, Guid lessonId)
    {
        // Lấy ID thật của sinh viên bằng claim Auth (Ví dụ mock 1 studentId từ route/auth nếu có, ở đây lấy user login)
        Guid studentId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await submissionService.StartExamAsync(classId, lessonId, studentId);
        return Ok(result);
    }

    [HttpPost("submit")]
    [EndpointSummary("Nộp bài tập/bài kiểm tra")]
    [EndpointDescription("Lưu trữ bài làm hoặc câu trả lời của học viên.")]
    public async Task<IActionResult> SubmitWork([FromBody] CreateSubmissionRequestDto request)
    {
        var result = await submissionService.SubmitWorkAsync(request);
        return Ok(result);
    }

    [HttpPut("{id:guid}/grade")]
    [EndpointSummary("Chấm điểm")]
    [EndpointDescription("Giảng viên thực hiện chấm điểm bài nộp của học viên.")]
    public async Task<IActionResult> Grade(Guid id, [FromBody] GradeSubmissionRequestDto request)
    {
        var isGraded = await submissionService.GradeSubmissionAsync(id, request);
        if (!isGraded) return NotFound("Không tìm thấy bài nộp.");
        return NoContent();
    }

    [HttpPost("submit-quiz")]
    [EndpointSummary("Nộp bài tập/bài kiểm tra")]
    [EndpointDescription("Lưu trữ bài làm hoặc câu trả lời của học viên.")]
    public async Task<IActionResult> SubmitQuiz([FromBody] SubmitQuizRequestDto request)
    {
        var result = await submissionService.SubmitQuizAsync(request);
        return Ok(result);
    }

    [HttpGet("lesson/{lessonId:guid}/export")]
    [EndpointSummary("Xuất bảng điểm ra file Excel")]
    [EndpointDescription("Xuất toàn bộ dữ liệu ra một file Excel (.xlsx).")]
    public async Task<IActionResult> ExportScores(Guid lessonId)
    {
        var fileBytes = await submissionService.ExportScoresToExcelAsync(lessonId);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Bang_Diem.xlsx");
    }
}