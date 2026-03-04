using ELearning.Core.DTOs.Course;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    // GET: api/courses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetAll()
    {
        var courses = await _courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    // GET: api/courses/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CourseResponseDto>> GetById(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound(new { message = "Không tìm thấy khóa học" });
        return Ok(course);
    }

    // POST: api/courses
    [HttpPost]
    public async Task<ActionResult<CourseResponseDto>> Create([FromBody] CreateCourseRequestDto request)
    {
        var newCourse = await _courseService.CreateCourseAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newCourse.Id }, newCourse);
    }

    // PUT: api/courses/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseRequestDto request)
    {
        var isUpdated = await _courseService.UpdateCourseAsync(id, request);
        if (!isUpdated) return NotFound(new { message = "Không tìm thấy khóa học để cập nhật" });
        return NoContent(); // Code 204: Cập nhật thành công nhưng không trả về data
    }

    // DELETE: api/courses/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _courseService.DeleteCourseAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy khóa học để xóa" });
        return NoContent();
    }
}