using System.Linq.Expressions;
using ELearning.Core.DTOs.Class;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using Xunit;

namespace ELearning.Tests.Services;

public class ClassLessonScheduleServiceTests
{
    private readonly Mock<IGenericRepository<ClassLessonSchedule>> _mockScheduleRepo;
    private readonly ClassLessonScheduleService _scheduleService;

    public ClassLessonScheduleServiceTests()
    {
        _mockScheduleRepo = new Mock<IGenericRepository<ClassLessonSchedule>>();
        _scheduleService = new ClassLessonScheduleService(_mockScheduleRepo.Object);
    }

    [Fact]
    public async Task GetScheduleAsync_ShouldReturnSchedule_WhenExists()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var schedule = new ClassLessonSchedule
        {
            ClassId = classId,
            LessonId = lessonId,
            StartTime = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(1),
            OverrideDuration = 60
        };

        _mockScheduleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClassLessonSchedule, bool>>>()))
                         .ReturnsAsync(new List<ClassLessonSchedule> { schedule });

        // Act
        var result = await _scheduleService.GetScheduleAsync(classId, lessonId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(classId, result.ClassId);
        Assert.Equal(lessonId, result.LessonId);
        Assert.Equal(60, result.OverrideDuration);
    }

    [Fact]
    public async Task GetScheduleAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();

        _mockScheduleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClassLessonSchedule, bool>>>()))
                         .ReturnsAsync(new List<ClassLessonSchedule>());

        // Act
        var result = await _scheduleService.GetScheduleAsync(classId, lessonId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsertScheduleAsync_ShouldCreateNew_WhenNotExists()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var request = new UpsertClassLessonScheduleRequestDto(DateTime.UtcNow, DateTime.UtcNow.AddDays(2), 120);

        _mockScheduleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClassLessonSchedule, bool>>>()))
                         .ReturnsAsync(new List<ClassLessonSchedule>());
                         
        _mockScheduleRepo.Setup(r => r.AddAsync(It.IsAny<ClassLessonSchedule>())).Returns(Task.CompletedTask);
        _mockScheduleRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _scheduleService.UpsertScheduleAsync(classId, lessonId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(120, result.OverrideDuration);
        _mockScheduleRepo.Verify(r => r.AddAsync(It.IsAny<ClassLessonSchedule>()), Times.Once);
        _mockScheduleRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpsertScheduleAsync_ShouldUpdateExisting_WhenExists()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var existingSchedule = new ClassLessonSchedule
        {
            ClassId = classId,
            LessonId = lessonId,
            StartTime = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(1),
            OverrideDuration = 60
        };

        var request = new UpsertClassLessonScheduleRequestDto(DateTime.UtcNow, DateTime.UtcNow.AddDays(3), 90);

        _mockScheduleRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClassLessonSchedule, bool>>>()))
                         .ReturnsAsync(new List<ClassLessonSchedule> { existingSchedule });
                         
        _mockScheduleRepo.Setup(r => r.Update(existingSchedule));
        _mockScheduleRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _scheduleService.UpsertScheduleAsync(classId, lessonId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(90, result.OverrideDuration);
        Assert.Equal(90, existingSchedule.OverrideDuration); // Make sure the property was updated
        _mockScheduleRepo.Verify(r => r.Update(existingSchedule), Times.Once);
        _mockScheduleRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
