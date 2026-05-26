using ELearning.Core.DTOs.Submission;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ELearning.Tests.Services;

public class SubmissionServiceTests
{
    private readonly Mock<IGenericRepository<Submission>> _mockSubmissionRepo;
    private readonly SubmissionService _submissionService;

    public SubmissionServiceTests()
    {
        _mockSubmissionRepo = new Mock<IGenericRepository<Submission>>();
        // Passing null for AppDbContext because we are testing methods that only use submissionRepo
        _submissionService = new SubmissionService(null!, _mockSubmissionRepo.Object);
    }

    [Fact]
    public async Task GetSubmissionAsync_ShouldReturnSubmission_WhenExists()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        
        var expectedSubs = new List<Submission>
        {
            new Submission { Id = Guid.NewGuid(), ClassId = classId, LessonId = lessonId, StudentId = studentId, IsSubmitted = true }
        };

        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(expectedSubs);

        // Act
        var result = await _submissionService.GetSubmissionAsync(classId, lessonId, studentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSubs[0].Id, result.Id);
    }

    [Fact]
    public async Task GetSubmissionAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(new List<Submission>());

        // Act
        var result = await _submissionService.GetSubmissionAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task StartExamAsync_ShouldReturnExistingSubmission_AndSetStartedAt()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var lessonId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var existingSub = new Submission { Id = Guid.NewGuid(), ClassId = classId, LessonId = lessonId, StudentId = studentId, StartedAt = null };

        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(new List<Submission> { existingSub });
                           
        _mockSubmissionRepo.Setup(r => r.Update(It.IsAny<Submission>()));
        _mockSubmissionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _submissionService.StartExamAsync(classId, lessonId, studentId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(existingSub.StartedAt);
        _mockSubmissionRepo.Verify(r => r.Update(existingSub), Times.Once);
        _mockSubmissionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartExamAsync_ShouldCreateNewSubmission_WhenNotExists()
    {
        // Arrange
        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(new List<Submission>());
                           
        _mockSubmissionRepo.Setup(r => r.AddAsync(It.IsAny<Submission>())).Returns(Task.CompletedTask);
        _mockSubmissionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _submissionService.StartExamAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.StartedAt);
        Assert.False(result.IsSubmitted);
        _mockSubmissionRepo.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Once);
        _mockSubmissionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitWorkAsync_ShouldUpdateExistingSubmission()
    {
        // Arrange
        var request = new CreateSubmissionRequestDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "http://url", "Notes");
        var existingSub = new Submission { Id = Guid.NewGuid(), IsSubmitted = false };

        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(new List<Submission> { existingSub });
                           
        _mockSubmissionRepo.Setup(r => r.Update(It.IsAny<Submission>()));
        _mockSubmissionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _submissionService.SubmitWorkAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(existingSub.IsSubmitted);
        Assert.Equal("http://url", existingSub.SubmissionUrl);
        _mockSubmissionRepo.Verify(r => r.Update(existingSub), Times.Once);
        _mockSubmissionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitWorkAsync_ShouldCreateNewSubmission_WhenNotExists()
    {
        // Arrange
        var request = new CreateSubmissionRequestDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "http://url", "Notes");

        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(new List<Submission>());
                           
        _mockSubmissionRepo.Setup(r => r.AddAsync(It.IsAny<Submission>())).Returns(Task.CompletedTask);
        _mockSubmissionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _submissionService.SubmitWorkAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSubmitted);
        Assert.Equal("http://url", result.SubmissionUrl);
        _mockSubmissionRepo.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Once);
        _mockSubmissionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GradeSubmissionAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var existingSub = new Submission { Id = subId };
        var request = new GradeSubmissionRequestDto(9.5f, "Good job");

        _mockSubmissionRepo.Setup(r => r.GetByIdAsync(subId)).ReturnsAsync(existingSub);
        _mockSubmissionRepo.Setup(r => r.Update(It.IsAny<Submission>()));
        _mockSubmissionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _submissionService.GradeSubmissionAsync(subId, request);

        // Assert
        Assert.True(result);
        Assert.Equal(9.5f, existingSub.Score);
        Assert.Equal("Good job", existingSub.Feedback);
        _mockSubmissionRepo.Verify(r => r.Update(existingSub), Times.Once);
        _mockSubmissionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GradeSubmissionAsync_ShouldReturnFalse_WhenSubmissionNotFound()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var request = new GradeSubmissionRequestDto(9.5f, "Good job");

        _mockSubmissionRepo.Setup(r => r.GetByIdAsync(subId)).ReturnsAsync((Submission)null!);

        // Act
        var result = await _submissionService.GradeSubmissionAsync(subId, request);

        // Assert
        Assert.False(result);
        _mockSubmissionRepo.Verify(r => r.Update(It.IsAny<Submission>()), Times.Never);
    }

    [Fact]
    public async Task SubmitQuizAsync_ShouldUpdateExistingSubmission()
    {
        // Arrange
        var request = new SubmitQuizRequestDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 8.0f, "{}", 1, true);
        var existingSub = new Submission { Id = Guid.NewGuid(), IsSubmitted = false };

        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(new List<Submission> { existingSub });
                           
        _mockSubmissionRepo.Setup(r => r.Update(It.IsAny<Submission>()));
        _mockSubmissionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _submissionService.SubmitQuizAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(existingSub.IsSubmitted);
        Assert.Equal(8.0f, existingSub.Score);
        Assert.Equal("{}", existingSub.QuizAnswersJson);
        Assert.Equal(1, existingSub.CheatWarnings);
        _mockSubmissionRepo.Verify(r => r.Update(existingSub), Times.Once);
        _mockSubmissionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitQuizAsync_ShouldCreateNewSubmission_WhenNotExists()
    {
        // Arrange
        var request = new SubmitQuizRequestDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 8.0f, "{}", 1, true);

        _mockSubmissionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Submission, bool>>>()))
                           .ReturnsAsync(new List<Submission>());
                           
        _mockSubmissionRepo.Setup(r => r.AddAsync(It.IsAny<Submission>())).Returns(Task.CompletedTask);
        _mockSubmissionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _submissionService.SubmitQuizAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSubmitted);
        Assert.Equal(8.0f, result.Score);
        Assert.Equal("{}", result.QuizAnswersJson);
        Assert.Equal(1, result.CheatWarnings);
        _mockSubmissionRepo.Verify(r => r.AddAsync(It.IsAny<Submission>()), Times.Once);
        _mockSubmissionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
