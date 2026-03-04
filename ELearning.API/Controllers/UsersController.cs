using ELearning.Core.DTOs.User;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // Lấy danh sách toàn bộ người dùng (Có thể lọc theo Role sau này)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });
        return Ok(user);
    }

    // Admin tạo người dùng mới
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserRequestDto request)
    {
        var newUser = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    // Cập nhật thông tin (ví dụ: Đổi Role, cập nhật Lớp hành chính)
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        var isUpdated = await _userService.UpdateUserAsync(id, request);
        if (!isUpdated) return NotFound(new { message = "Không tìm thấy người dùng" });
        return NoContent();
    }

    // Khóa/Xóa tài khoản
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await _userService.DeleteUserAsync(id);
        if (!isDeleted) return NotFound();
        return NoContent();
    }
}