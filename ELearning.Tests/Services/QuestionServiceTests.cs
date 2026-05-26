using ELearning.Core.DTOs.Question;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ELearning.Tests.Services;

public class QuestionServiceTests
{
    private readonly Mock<IGenericRepository<Question>> _mockQuestionRepo;
    private readonly QuestionService _questionService;

    public QuestionServiceTests()
    {
        _mockQuestionRepo = new Mock<IGenericRepository<Question>>();
        _questionService = new QuestionService(_mockQuestionRepo.Object);
    }

    [Fact]
    public async Task GetQuestionsByLessonIdAsync_ShouldReturnQuestions()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var expectedQuestions = new List<Question>
        {
            new Question { Id = Guid.NewGuid(), LessonId = lessonId, Content = "Q1", CorrectOption = "A" },
            new Question { Id = Guid.NewGuid(), LessonId = lessonId, Content = "Q2", CorrectOption = "B" }
        };

        _mockQuestionRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Question, bool>>>()))
                         .ReturnsAsync(expectedQuestions);

        // Act
        var result = await _questionService.GetQuestionsByLessonIdAsync(lessonId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task CreateQuestionAsync_ShouldReturnNewQuestion()
    {
        // Arrange
        var request = new CreateQuestionRequestDto(Guid.NewGuid(), "New Question", "A", "B", "C", "D", "A", "Expl");
        
        _mockQuestionRepo.Setup(r => r.AddAsync(It.IsAny<Question>())).Returns(Task.CompletedTask);
        _mockQuestionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _questionService.CreateQuestionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Question", result.Content);
        _mockQuestionRepo.Verify(r => r.AddAsync(It.IsAny<Question>()), Times.Once);
        _mockQuestionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestionAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var existingQuestion = new Question { Id = questionId, Content = "Old" };
        var request = new UpdateQuestionRequestDto("New", "A", "B", "C", "D", "A", "Expl");

        _mockQuestionRepo.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync(existingQuestion);
        _mockQuestionRepo.Setup(r => r.Update(It.IsAny<Question>()));
        _mockQuestionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _questionService.UpdateQuestionAsync(questionId, request);

        // Assert
        Assert.True(result);
        Assert.Equal("New", existingQuestion.Content);
        _mockQuestionRepo.Verify(r => r.Update(existingQuestion), Times.Once);
        _mockQuestionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestionAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var request = new UpdateQuestionRequestDto("New", "A", "B", "C", "D", "A", "Expl");

        _mockQuestionRepo.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync((Question)null!);

        // Act
        var result = await _questionService.UpdateQuestionAsync(questionId, request);

        // Assert
        Assert.False(result);
        _mockQuestionRepo.Verify(r => r.Update(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task DeleteQuestionAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var existingQuestion = new Question { Id = questionId };

        _mockQuestionRepo.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync(existingQuestion);
        _mockQuestionRepo.Setup(r => r.Delete(existingQuestion));
        _mockQuestionRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _questionService.DeleteQuestionAsync(questionId);

        // Assert
        Assert.True(result);
        _mockQuestionRepo.Verify(r => r.Delete(existingQuestion), Times.Once);
        _mockQuestionRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestionAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _mockQuestionRepo.Setup(r => r.GetByIdAsync(questionId)).ReturnsAsync((Question)null!);

        // Act
        var result = await _questionService.DeleteQuestionAsync(questionId);

        // Assert
        Assert.False(result);
        _mockQuestionRepo.Verify(r => r.Delete(It.IsAny<Question>()), Times.Never);
    }
}
