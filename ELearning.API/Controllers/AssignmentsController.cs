using ELearning.Core.DTOs.Assignment;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy toàn bộ danh sách bài tập")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất danh sách tất cả các bài tập có sẵn trong hệ thống.")]
    public async Task<ActionResult<IEnumerable<AssignmentResponseDto>>> GetAll()
    {
        var assignments = await _assignmentService.GetAllAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpGet("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy chi tiết bằng ID")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất thông tin chi tiết của một đối tượng cụ thể thông qua ID.")]
    public async Task<ActionResult<AssignmentResponseDto>> GetById(Guid id)
    {
        var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
        if (assignment == null) return NotFound(new { message = "Không tìm thấy bài tập" });
        return Ok(assignment);
    }

    [HttpPost]
    [Microsoft.AspNetCore.Http.EndpointSummary("Tạo mới bài tập")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Tạo một bài tập mới trong hệ thống.")]
    public async Task<ActionResult<AssignmentResponseDto>> Create([FromBody] CreateAssignmentRequestDto request)
    {
        var newAssignment = await _assignmentService.CreateAssignmentAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newAssignment.Id }, newAssignment);
    }

    [HttpPut("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Cập nhật bài tập")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Cập nhật thông tin của một bài tập đã tồn tại.")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssignmentRequestDto request)
    {
        var isUpdated = await _assignmentService.UpdateAssignmentAsync(id, request);
        if (!isUpdated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Xóa bài tập")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Xóa vĩnh viễn hoặc khóa một bài tập khỏi hệ thống.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _assignmentService.DeleteAssignmentAsync(id);
        if (!isDeleted) return NotFound();
        return NoContent();
    }
}