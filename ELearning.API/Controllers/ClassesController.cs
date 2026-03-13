using ELearning.Core.DTOs.Class;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _classService.GetAllClassesAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClassRequestDto request)
    {
        return Ok(await _classService.CreateClassAsync(request));
    }

    [HttpPost("{id:guid}/enroll")]
    public async Task<IActionResult> EnrollStudent(Guid id, [FromBody] EnrollStudentRequestDto request)
    {
        var success = await _classService.EnrollStudentAsync(id, request.StudentId);
        if (!success) return BadRequest("Lỗi khi ghi danh sinh viên.");
        return Ok(new { message = "Ghi danh thành công!" });
    }
}