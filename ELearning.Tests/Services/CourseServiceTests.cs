using ELearning.Core.DTOs.Course;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ELearning.Tests.Services;

public class CourseServiceTests
{
    private readonly Mock<IGenericRepository<Course>> _mockCourseRepo;
    private readonly CourseService _courseService;

    public CourseServiceTests()
    {
        _mockCourseRepo = new Mock<IGenericRepository<Course>>();
        
        // Pass null for AppDbContext because we only test methods that use generic repository
        _courseService = new CourseService(_mockCourseRepo.Object, null!);
    }

    [Fact]
    public async Task CreateCourseAsync_ShouldCreateNewCourse()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var request = new CreateCourseRequestDto("C# Mastery", "Learn C# from scratch", "http://thumb", true);

        _mockCourseRepo.Setup(r => r.AddAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);
        _mockCourseRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _courseService.CreateCourseAsync(request, creatorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("C# Mastery", result.Title);
        Assert.Equal(creatorId, result.CreatorId);
        Assert.True(result.IsPublic);
        _mockCourseRepo.Verify(r => r.AddAsync(It.IsAny<Course>()), Times.Once);
        _mockCourseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = new Course { Id = courseId, Title = "Old Title" };
        var request = new UpdateCourseRequestDto("New Title", "New Desc", "http://new", false);

        _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(existingCourse);
        _mockCourseRepo.Setup(r => r.Update(It.IsAny<Course>()));
        _mockCourseRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _courseService.UpdateCourseAsync(courseId, request);

        // Assert
        Assert.True(result);
        Assert.Equal("New Title", existingCourse.Title);
        Assert.False(existingCourse.IsPublic);
        _mockCourseRepo.Verify(r => r.Update(existingCourse), Times.Once);
        _mockCourseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCourseAsync_ShouldReturnFalse_WhenCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new UpdateCourseRequestDto("New Title", "New Desc", "http://new", false);

        _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course)null!);

        // Act
        var result = await _courseService.UpdateCourseAsync(courseId, request);

        // Assert
        Assert.False(result);
        _mockCourseRepo.Verify(r => r.Update(It.IsAny<Course>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCourseAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var existingCourse = new Course { Id = courseId };

        _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(existingCourse);
        _mockCourseRepo.Setup(r => r.Delete(existingCourse));
        _mockCourseRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _courseService.DeleteCourseAsync(courseId);

        // Assert
        Assert.True(result);
        _mockCourseRepo.Verify(r => r.Delete(existingCourse), Times.Once);
        _mockCourseRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCourseAsync_ShouldReturnFalse_WhenCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course)null!);

        // Act
        var result = await _courseService.DeleteCourseAsync(courseId);

        // Assert
        Assert.False(result);
        _mockCourseRepo.Verify(r => r.Delete(It.IsAny<Course>()), Times.Never);
    }
}
