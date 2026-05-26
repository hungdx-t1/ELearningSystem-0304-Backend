using ELearning.Core.DTOs.Chapter;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ELearning.Tests.Services;

public class ChapterServiceTests
{
    private readonly Mock<IGenericRepository<Chapter>> _mockChapterRepo;
    private readonly ChapterService _chapterService;

    public ChapterServiceTests()
    {
        _mockChapterRepo = new Mock<IGenericRepository<Chapter>>();
        _chapterService = new ChapterService(_mockChapterRepo.Object);
    }

    [Fact]
    public async Task GetChaptersByCourseIdAsync_ShouldReturnSortedChapters()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var chapters = new List<Chapter>
        {
            new Chapter { Id = Guid.NewGuid(), CourseId = courseId, Title = "Chapter 2", SortOrder = 2 },
            new Chapter { Id = Guid.NewGuid(), CourseId = courseId, Title = "Chapter 1", SortOrder = 1 }
        };

        _mockChapterRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Chapter, bool>>>()))
                        .ReturnsAsync(chapters);

        // Act
        var result = await _chapterService.GetChaptersByCourseIdAsync(courseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Chapter 1", result.First().Title);
        Assert.Equal("Chapter 2", result.Last().Title);
    }

    [Fact]
    public async Task GetChapterByIdAsync_ShouldReturnChapter_WhenExists()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var chapter = new Chapter { Id = chapterId, Title = "Test Chapter" };

        _mockChapterRepo.Setup(r => r.GetByIdAsync(chapterId)).ReturnsAsync(chapter);

        // Act
        var result = await _chapterService.GetChapterByIdAsync(chapterId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(chapterId, result.Id);
        Assert.Equal("Test Chapter", result.Title);
    }

    [Fact]
    public async Task GetChapterByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        _mockChapterRepo.Setup(r => r.GetByIdAsync(chapterId)).ReturnsAsync((Chapter)null!);

        // Act
        var result = await _chapterService.GetChapterByIdAsync(chapterId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateChapterAsync_ShouldCalculateSortOrderAndReturnChapter()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new CreateChapterRequestDto(courseId, "New Chapter", 0);

        var existingChapters = new List<Chapter>
        {
            new Chapter { Id = Guid.NewGuid(), CourseId = courseId, SortOrder = 5 }
        };

        _mockChapterRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Chapter, bool>>>()))
                        .ReturnsAsync(existingChapters);
                        
        _mockChapterRepo.Setup(r => r.AddAsync(It.IsAny<Chapter>())).Returns(Task.CompletedTask);
        _mockChapterRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _chapterService.CreateChapterAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Chapter", result.Title);
        Assert.Equal(6, result.SortOrder); // Max(5) + 1
        _mockChapterRepo.Verify(r => r.AddAsync(It.IsAny<Chapter>()), Times.Once);
        _mockChapterRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateChapterAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var existingChapter = new Chapter { Id = chapterId, Title = "Old Title" };
        var request = new UpdateChapterRequestDto("New Title", 2);

        _mockChapterRepo.Setup(r => r.GetByIdAsync(chapterId)).ReturnsAsync(existingChapter);
        _mockChapterRepo.Setup(r => r.Update(It.IsAny<Chapter>()));
        _mockChapterRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _chapterService.UpdateChapterAsync(chapterId, request);

        // Assert
        Assert.True(result);
        Assert.Equal("New Title", existingChapter.Title);
        Assert.Equal(2, existingChapter.SortOrder);
        _mockChapterRepo.Verify(r => r.Update(existingChapter), Times.Once);
        _mockChapterRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateChapterAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var request = new UpdateChapterRequestDto("New Title", 2);

        _mockChapterRepo.Setup(r => r.GetByIdAsync(chapterId)).ReturnsAsync((Chapter)null!);

        // Act
        var result = await _chapterService.UpdateChapterAsync(chapterId, request);

        // Assert
        Assert.False(result);
        _mockChapterRepo.Verify(r => r.Update(It.IsAny<Chapter>()), Times.Never);
    }

    [Fact]
    public async Task DeleteChapterAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var existingChapter = new Chapter { Id = chapterId };

        _mockChapterRepo.Setup(r => r.GetByIdAsync(chapterId)).ReturnsAsync(existingChapter);
        _mockChapterRepo.Setup(r => r.Delete(existingChapter));
        _mockChapterRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _chapterService.DeleteChapterAsync(chapterId);

        // Assert
        Assert.True(result);
        _mockChapterRepo.Verify(r => r.Delete(existingChapter), Times.Once);
        _mockChapterRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteChapterAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var chapterId = Guid.NewGuid();

        _mockChapterRepo.Setup(r => r.GetByIdAsync(chapterId)).ReturnsAsync((Chapter)null!);

        // Act
        var result = await _chapterService.DeleteChapterAsync(chapterId);

        // Assert
        Assert.False(result);
        _mockChapterRepo.Verify(r => r.Delete(It.IsAny<Chapter>()), Times.Never);
    }
}
