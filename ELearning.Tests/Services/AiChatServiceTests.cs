using System.Linq.Expressions;
using ELearning.Core.DTOs.AiChatLog;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Services.Implements;
using Moq;
using Pgvector;
using Xunit;

namespace ELearning.Tests.Services;

public class AiChatServiceTests
{
    private readonly Mock<IGenericRepository<AiChatLog>> _mockLogRepo;
    private readonly Mock<IAiService> _mockAiService;
    private readonly AiChatService _aiChatService;

    public AiChatServiceTests()
    {
        _mockLogRepo = new Mock<IGenericRepository<AiChatLog>>();
        _mockAiService = new Mock<IAiService>();

        // We pass null for AppDbContext because we only test methods that don't depend on EF Core specific translations
        _aiChatService = new AiChatService(_mockLogRepo.Object, _mockAiService.Object, null!);
    }

    [Fact]
    public async Task GetUserChatHistoryAsync_ShouldReturnSortedLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var logs = new List<AiChatLog>
        {
            new AiChatLog { Id = Guid.NewGuid(), UserId = userId, Timestamp = DateTime.UtcNow },
            new AiChatLog { Id = Guid.NewGuid(), UserId = userId, Timestamp = DateTime.UtcNow.AddMinutes(-5) }
        };

        _mockLogRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AiChatLog, bool>>>()))
                    .ReturnsAsync(logs);

        // Act
        var result = await _aiChatService.GetUserChatHistoryAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        // The one from 5 mins ago should be first because it is OrderBy(Timestamp) ascending
        Assert.True(result.First().Timestamp < result.Last().Timestamp);
    }

    [Fact]
    public async Task LogChatAsync_ShouldGenerateEmbeddingAndSave()
    {
        // Arrange
        var request = new CreateAiChatLogDto(Guid.NewGuid(), "Hello AI", "Hello User");
        var mockVector = new Vector(new float[] { 0.1f, 0.2f, 0.3f });

        _mockAiService.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                      .ReturnsAsync(mockVector);

        _mockLogRepo.Setup(r => r.AddAsync(It.IsAny<AiChatLog>())).Returns(Task.CompletedTask);
        _mockLogRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _aiChatService.LogChatAsync(request);

        // Assert
        _mockAiService.Verify(s => s.GenerateEmbeddingAsync(It.Is<string>(str => str.Contains("Hello AI") && str.Contains("Hello User"))), Times.Once);
        _mockLogRepo.Verify(r => r.AddAsync(It.Is<AiChatLog>(l => l.Message == "Hello AI" && l.Embedding == mockVector)), Times.Once);
        _mockLogRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LogChatAsync_ShouldSaveWithNullEmbedding_WhenAiServiceFails()
    {
        // Arrange
        var request = new CreateAiChatLogDto(Guid.NewGuid(), "Hello AI", "Hello User");

        _mockAiService.Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>()))
                      .ThrowsAsync(new Exception("AI service error"));

        _mockLogRepo.Setup(r => r.AddAsync(It.IsAny<AiChatLog>())).Returns(Task.CompletedTask);
        _mockLogRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _aiChatService.LogChatAsync(request);

        // Assert
        _mockAiService.Verify(s => s.GenerateEmbeddingAsync(It.IsAny<string>()), Times.Once);
        _mockLogRepo.Verify(r => r.AddAsync(It.Is<AiChatLog>(l => l.Message == "Hello AI" && l.Embedding == null)), Times.Once);
        _mockLogRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
