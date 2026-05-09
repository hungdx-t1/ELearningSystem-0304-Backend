using ELearning.Core.DTOs;
using ELearning.Core.DTOs.User;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await userService.GetUsersPaginatedAsync(search, role, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });
        return Ok(user);
    }

    // Admin tạo người dùng mới
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserRequestDto request)
    {
        var newUser = await userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }

    // Cập nhật thông tin (ví dụ: Đổi Role, cập nhật Lớp hành chính)
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        var isUpdated = await userService.UpdateUserAsync(id, request);
        if (!isUpdated) return NotFound(new { message = "Không tìm thấy người dùng" });
        return NoContent();
    }

    // Xóa tài khoản
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var isDeleted = await userService.DeleteUserAsync(id);
        if (!isDeleted) return NotFound(new { message = "Không tìm thấy người dùng" });
        return NoContent();
    }

    // khóa/mở tài khoản
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var isToggled = await userService.ToggleUserStatusAsync(id);
        if (!isToggled) return NotFound(new { message = "Không tìm thấy người dùng" });

        return Ok(new { message = "Đã thay đổi trạng thái thành công" });
    }
}