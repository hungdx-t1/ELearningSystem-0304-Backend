using ELearning.API.Controllers;
using ELearning.Core.DTOs.Chapter;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ELearning.Tests.Controllers;

public class ChaptersControllerTests
{
    private readonly Mock<IChapterService> _mockChapterService;
    private readonly ChaptersController _controller;

    public ChaptersControllerTests()
    {
        _mockChapterService = new Mock<IChapterService>();
        _controller = new ChaptersController(_mockChapterService.Object);
    }

    [Fact]
    public async Task GetChaptersByCourse_ShouldReturnOk_WithChaptersList()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var expectedChapters = new List<ChapterResponseDto>
        {
            new ChapterResponseDto(Guid.NewGuid(), courseId, "Chapter 1", 1),
            new ChapterResponseDto(Guid.NewGuid(), courseId, "Chapter 2", 2)
        };
        _mockChapterService.Setup(s => s.GetChaptersByCourseIdAsync(courseId))
                           .ReturnsAsync(expectedChapters);

        // Act
        var result = await _controller.GetChaptersByCourse(courseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<ChapterResponseDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count());
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenChapterExists()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var expectedChapter = new ChapterResponseDto(chapterId, Guid.NewGuid(), "Chapter 1", 1);
        _mockChapterService.Setup(s => s.GetChapterByIdAsync(chapterId))
                           .ReturnsAsync(expectedChapter);

        // Act
        var result = await _controller.GetById(chapterId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<ChapterResponseDto>(okResult.Value);
        Assert.Equal(chapterId, returnValue.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenChapterDoesNotExist()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        _mockChapterService.Setup(s => s.GetChapterByIdAsync(chapterId))
                           .ReturnsAsync((ChapterResponseDto)null!);

        // Act
        var result = await _controller.GetById(chapterId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WithNewChapter()
    {
        // Arrange
        var request = new CreateChapterRequestDto(Guid.NewGuid(), "New Chapter", 1);
        var expectedChapter = new ChapterResponseDto(Guid.NewGuid(), request.CourseId, request.Title, request.SortOrder);
        
        _mockChapterService.Setup(s => s.CreateChapterAsync(request))
                           .ReturnsAsync(expectedChapter);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ChaptersController.GetById), createdAtActionResult.ActionName);
        var returnValue = Assert.IsType<ChapterResponseDto>(createdAtActionResult.Value);
        Assert.Equal(expectedChapter.Id, returnValue.Id);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var request = new UpdateChapterRequestDto("Updated Chapter", 1);
        _mockChapterService.Setup(s => s.UpdateChapterAsync(chapterId, request))
                           .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(chapterId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenChapterDoesNotExist()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var request = new UpdateChapterRequestDto("Updated Chapter", 1);
        _mockChapterService.Setup(s => s.UpdateChapterAsync(chapterId, request))
                           .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(chapterId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenDeleteIsSuccessful()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        _mockChapterService.Setup(s => s.DeleteChapterAsync(chapterId))
                           .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(chapterId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenChapterDoesNotExist()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        _mockChapterService.Setup(s => s.DeleteChapterAsync(chapterId))
                           .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(chapterId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }
}
