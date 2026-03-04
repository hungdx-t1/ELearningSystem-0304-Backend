using ELearning.Core.DTOs.Assignment;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public AssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssignmentResponseDto>>> GetAll()
    {
        var assignments = await _assignmentService.GetAllAssignmentsAsync();
        return Ok(assignments);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssignmentResponseDto>> GetById(Guid id)
    {
        var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
        if (assignment == null) return NotFound(new { message = "Không tìm thấy bài tập" });
        return Ok(assignment);
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentResponseDto>> Create([FromBody] CreateAssignmentRequestDto request)
    {
        var newAssignment = await _assignmentService.CreateAssignmentAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newAssignment.Id }, newAssignment);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssignmentRequestDto request)
    {
        var isUpdated = await _assignmentService.UpdateAssignmentAsync(id, request);
        if (!isUpdated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _assignmentService.DeleteAssignmentAsync(id);
        if (!isDeleted) return NotFound();
        return NoContent();
    }
}