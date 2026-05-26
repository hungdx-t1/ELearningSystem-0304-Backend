using ELearning.Core.DTOs.Lesson;
using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ELearning.Tests.Services;

public class LessonServiceTests
{
    private readonly Mock<IGenericRepository<Lesson>> _mockLessonRepo;
    private readonly LessonService _lessonService;

    public LessonServiceTests()
    {
        _mockLessonRepo = new Mock<IGenericRepository<Lesson>>();
        _lessonService = new LessonService(_mockLessonRepo.Object);
    }

    [Fact]
    public async Task GetLessonsByChapterIdAsync_ShouldReturnSortedLessons()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var lessons = new List<Lesson>
        {
            new Lesson { Id = Guid.NewGuid(), ChapterId = chapterId, Title = "Lesson 2", SortOrder = 2 },
            new Lesson { Id = Guid.NewGuid(), ChapterId = chapterId, Title = "Lesson 1", SortOrder = 1 }
        };

        _mockLessonRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Lesson, bool>>>()))
                       .ReturnsAsync(lessons);

        // Act
        var result = await _lessonService.GetLessonsByChapterIdAsync(chapterId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Lesson 1", result.First().Title); // SortOrder 1 should be first
        Assert.Equal("Lesson 2", result.Last().Title);  // SortOrder 2 should be last
    }

    [Fact]
    public async Task GetLessonByIdAsync_ShouldReturnLesson_WhenExists()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var expectedLesson = new Lesson { Id = lessonId, Title = "Test Lesson" };

        _mockLessonRepo.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(expectedLesson);

        // Act
        var result = await _lessonService.GetLessonByIdAsync(lessonId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lessonId, result.Id);
        Assert.Equal("Test Lesson", result.Title);
    }

    [Fact]
    public async Task GetLessonByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        _mockLessonRepo.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync((Lesson)null!);

        // Act
        var result = await _lessonService.GetLessonByIdAsync(lessonId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldCalculateSortOrderAndReturnLesson()
    {
        // Arrange
        var chapterId = Guid.NewGuid();
        var request = new CreateLessonRequestDto(chapterId, "New Lesson", LessonType.Document, false, null, null, "http://doc", 10, 0); // SortOrder = 0

        var existingLessons = new List<Lesson>
        {
            new Lesson { Id = Guid.NewGuid(), ChapterId = chapterId, SortOrder = 5 }
        };

        _mockLessonRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Lesson, bool>>>()))
                       .ReturnsAsync(existingLessons);
                       
        _mockLessonRepo.Setup(r => r.AddAsync(It.IsAny<Lesson>())).Returns(Task.CompletedTask);
        _mockLessonRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _lessonService.CreateLessonAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.SortOrder); // Should be max + 1
        _mockLessonRepo.Verify(r => r.AddAsync(It.IsAny<Lesson>()), Times.Once);
        _mockLessonRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldThrowArgumentException_WhenVideoUrlMissingForVideoLesson()
    {
        // Arrange
        var request = new CreateLessonRequestDto(Guid.NewGuid(), "Video Lesson", LessonType.Video, false, VideoProvider.Youtube, null, null, 10, 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _lessonService.CreateLessonAsync(request));
        Assert.Contains("Bài học dạng Video bắt buộc phải có VideoUrl", exception.Message);
    }

    [Fact]
    public async Task CreateLessonAsync_ShouldThrowArgumentException_WhenDocumentUrlMissingForDocumentLesson()
    {
        // Arrange
        var request = new CreateLessonRequestDto(Guid.NewGuid(), "Doc Lesson", LessonType.Document, false, null, null, null, 10, 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _lessonService.CreateLessonAsync(request));
        Assert.Contains("Bài học dạng Tài liệu bắt buộc phải có DocumentUrl", exception.Message);
    }

    [Fact]
    public async Task UpdateLessonAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var existingLesson = new Lesson { Id = lessonId, Title = "Old Title" };
        var request = new UpdateLessonRequestDto("New Title", LessonType.Document, false, null, null, null, 15, 2);

        _mockLessonRepo.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(existingLesson);
        _mockLessonRepo.Setup(r => r.Update(It.IsAny<Lesson>()));
        _mockLessonRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _lessonService.UpdateLessonAsync(lessonId, request);

        // Assert
        Assert.True(result);
        Assert.Equal("New Title", existingLesson.Title);
        Assert.Equal(2, existingLesson.SortOrder);
        _mockLessonRepo.Verify(r => r.Update(existingLesson), Times.Once);
        _mockLessonRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateLessonAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var request = new UpdateLessonRequestDto("New Title", LessonType.Document, false, null, null, null, 15, 2);

        _mockLessonRepo.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync((Lesson)null!);

        // Act
        var result = await _lessonService.UpdateLessonAsync(lessonId, request);

        // Assert
        Assert.False(result);
        _mockLessonRepo.Verify(r => r.Update(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task DeleteLessonAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var existingLesson = new Lesson { Id = lessonId };

        _mockLessonRepo.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(existingLesson);
        _mockLessonRepo.Setup(r => r.Delete(existingLesson));
        _mockLessonRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _lessonService.DeleteLessonAsync(lessonId);

        // Assert
        Assert.True(result);
        _mockLessonRepo.Verify(r => r.Delete(existingLesson), Times.Once);
        _mockLessonRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteLessonAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();

        _mockLessonRepo.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync((Lesson)null!);

        // Act
        var result = await _lessonService.DeleteLessonAsync(lessonId);

        // Assert
        Assert.False(result);
        _mockLessonRepo.Verify(r => r.Delete(It.IsAny<Lesson>()), Times.Never);
    }

    [Fact]
    public async Task UpdateLessonOrdersAsync_ShouldUpdateAllLessonsAndReturnTrue()
    {
        // Arrange
        var lesson1Id = Guid.NewGuid();
        var lesson2Id = Guid.NewGuid();
        
        var request = new List<UpdateLessonOrderDto>
        {
            new UpdateLessonOrderDto(lesson1Id, 2),
            new UpdateLessonOrderDto(lesson2Id, 1)
        };

        var existingLessons = new List<Lesson>
        {
            new Lesson { Id = lesson1Id, SortOrder = 1 },
            new Lesson { Id = lesson2Id, SortOrder = 2 }
        };

        _mockLessonRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Lesson, bool>>>()))
                       .ReturnsAsync(existingLessons);
                       
        _mockLessonRepo.Setup(r => r.Update(It.IsAny<Lesson>()));
        _mockLessonRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _lessonService.UpdateLessonOrdersAsync(request);

        // Assert
        Assert.True(result);
        Assert.Equal(2, existingLessons.First(l => l.Id == lesson1Id).SortOrder);
        Assert.Equal(1, existingLessons.First(l => l.Id == lesson2Id).SortOrder);
        _mockLessonRepo.Verify(r => r.Update(It.IsAny<Lesson>()), Times.Exactly(2));
        _mockLessonRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
