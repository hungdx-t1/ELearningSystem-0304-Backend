using ELearning.API.Controllers;
using ELearning.Core.DTOs;
using ELearning.Core.DTOs.User;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ELearning.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new UsersController(_mockUserService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResult<UserResponseDto>
        {
            Items = new List<UserResponseDto> { new UserResponseDto(Guid.NewGuid(), "U1", "user 1", "user1@a.com", UserRole.Student, null, null, null, true, DateTime.UtcNow) },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };
        _mockUserService.Setup(s => s.GetUsersPaginatedAsync(It.IsAny<string>(), It.IsAny<string>(), 1, 10)).ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(null, null, 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedResult = Assert.IsType<PagedResult<UserResponseDto>>(okResult.Value);
        Assert.Single(returnedResult.Items);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync((UserResponseDto)null!);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("Không tìm thấy", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new UserResponseDto(id, "U1", "user 1", "user1@a.com", UserRole.Student, null, null, null, true, DateTime.UtcNow);
        _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(user);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedUser = Assert.IsType<UserResponseDto>(okResult.Value);
        Assert.Equal(id, returnedUser.Id);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateUserRequestDto("U1", "User 1", "user1@a.com", "pass", UserRole.Student, null);
        var createdUser = new UserResponseDto(Guid.NewGuid(), "U1", "User 1", "user1@a.com", UserRole.Student, null, null, null, true, DateTime.UtcNow);
        _mockUserService.Setup(s => s.CreateUserAsync(request)).ReturnsAsync(createdUser);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(UsersController.GetById), createdResult.ActionName);
        Assert.Equal(createdUser, createdResult.Value);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateUserRequestDto("U1_New", null, null, null, true);
        _mockUserService.Setup(s => s.UpdateUserAsync(id, request)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(id, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateUserRequestDto("U1_New", null, null, null, true);
        _mockUserService.Setup(s => s.UpdateUserAsync(id, request)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(id, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Không tìm thấy", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUserService.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUserService.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Không tìm thấy", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenInvalidOperationException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUserService.Setup(s => s.DeleteUserAsync(id)).ThrowsAsync(new InvalidOperationException("Không thể xóa user"));

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Không thể xóa", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task ToggleStatus_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUserService.Setup(s => s.ToggleUserStatusAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _controller.ToggleStatus(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Đã thay đổi trạng thái", okResult.Value?.ToString());
    }

    [Fact]
    public async Task ToggleStatus_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockUserService.Setup(s => s.ToggleUserStatusAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _controller.ToggleStatus(id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Không tìm thấy", notFoundResult.Value?.ToString());
    }
}
