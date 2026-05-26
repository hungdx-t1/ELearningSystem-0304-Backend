using ELearning.API.Controllers;
using ELearning.Core.DTOs.Lesson;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ELearning.Tests.Controllers;

public class LessonsControllerTests
{
    private readonly Mock<ILessonService> _mockLessonService;
    private readonly LessonsController _controller;

    public LessonsControllerTests()
    {
        _mockLessonService = new Mock<ILessonService>();
        _controller = new LessonsController(_mockLessonService.Object);
    }

    [Fact]
    public async Task GetLessonsByChapter_ShouldReturnOk_WithLessons()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var lessons = new List<LessonResponseDto>
        {
            new LessonResponseDto(Guid.NewGuid(), chapterId, "L1", LessonType.Video, false, null, null, null, null, 1)
        };
        _mockLessonService.Setup(s => s.GetLessonsByChapterIdAsync(chapterId)).ReturnsAsync(lessons);

        // Act
        var result = await _controller.GetLessonsByChapter(chapterId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLessons = Assert.IsAssignableFrom<IEnumerable<LessonResponseDto>>(okResult.Value);
        Assert.Single(returnedLessons);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenLessonDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockLessonService.Setup(s => s.GetLessonByIdAsync(id)).ReturnsAsync((LessonResponseDto)null!);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenLessonExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lesson = new LessonResponseDto(id, Guid.NewGuid(), "L1", LessonType.Video, false, null, null, null, null, 1);
        _mockLessonService.Setup(s => s.GetLessonByIdAsync(id)).ReturnsAsync(lesson);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedLesson = Assert.IsType<LessonResponseDto>(okResult.Value);
        Assert.Equal(id, returnedLesson.Id);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
    {
        // Arrange
        var request = new CreateLessonRequestDto(Guid.NewGuid(), "L1", LessonType.Video, false, null, "http://vid", null, 10, 1);
        var createdLesson = new LessonResponseDto(Guid.NewGuid(), request.ChapterId, "L1", LessonType.Video, false, null, "http://vid", null, 10, 1);
        _mockLessonService.Setup(s => s.CreateLessonAsync(request)).ReturnsAsync(createdLesson);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(LessonsController.GetById), createdResult.ActionName);
        Assert.Equal(createdLesson, createdResult.Value);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
    {
        // Arrange
        var request = new CreateLessonRequestDto(Guid.NewGuid(), "L1", LessonType.Video, false, null, null, null, 10, 1);
        _mockLessonService.Setup(s => s.CreateLessonAsync(request)).ThrowsAsync(new ArgumentException("Lỗi"));

        // Act
        var result = await _controller.Create(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Lỗi", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateLessonRequestDto("L1", LessonType.Video, false, null, "http://vid", null, 10, 1);
        _mockLessonService.Setup(s => s.UpdateLessonAsync(id, request)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(id, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenLessonDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateLessonRequestDto("L1", LessonType.Video, false, null, "http://vid", null, 10, 1);
        _mockLessonService.Setup(s => s.UpdateLessonAsync(id, request)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(id, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Không tìm thấy bài học", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateLessonRequestDto("L1", LessonType.Video, false, null, null, null, 10, 1);
        _mockLessonService.Setup(s => s.UpdateLessonAsync(id, request)).ThrowsAsync(new ArgumentException("Lỗi"));

        // Act
        var result = await _controller.Update(id, request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Lỗi", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockLessonService.Setup(s => s.DeleteLessonAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenLessonDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockLessonService.Setup(s => s.DeleteLessonAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("Không tìm thấy bài học", notFoundResult.Value?.ToString());
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var request = new List<UpdateLessonOrderDto>();
        _mockLessonService.Setup(s => s.UpdateLessonOrdersAsync(request)).ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateOrder(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturnBadRequest_WhenFails()
    {
        // Arrange
        var request = new List<UpdateLessonOrderDto>();
        _mockLessonService.Setup(s => s.UpdateLessonOrdersAsync(request)).ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateOrder(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Lỗi khi cập nhật thứ tự", badRequestResult.Value?.ToString());
    }
}
