using ELearning.API.Controllers;
using ELearning.Core.DTOs.AiChatLog;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ELearning.Tests.Controllers;

public class AiChatLogsControllerTests
{
    private readonly Mock<IAiChatService> _mockAiChatService;
    private readonly AiChatLogsController _controller;

    public AiChatLogsControllerTests()
    {
        _mockAiChatService = new Mock<IAiChatService>();
        _controller = new AiChatLogsController(_mockAiChatService.Object);
    }

    [Fact]
    public async Task GetHistory_ShouldReturnOk_WithChatLogs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var logs = new List<AiChatLogDto>
        {
            new AiChatLogDto(Guid.NewGuid(), userId, "Message 1", "Reply 1", DateTime.UtcNow)
        };
        _mockAiChatService.Setup(s => s.GetUserChatHistoryAsync(userId)).ReturnsAsync(logs);

        // Act
        var result = await _controller.GetHistory(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLogs = Assert.IsAssignableFrom<IEnumerable<AiChatLogDto>>(okResult.Value);
        Assert.Single(returnedLogs);
    }

    [Fact]
    public async Task SaveLog_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var request = new CreateAiChatLogDto(Guid.NewGuid(), "Msg", "Reply");
        _mockAiChatService.Setup(s => s.LogChatAsync(request)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SaveLog(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Đã lưu lịch sử chat", okResult.Value?.ToString());
        _mockAiChatService.Verify(s => s.LogChatAsync(request), Times.Once);
    }
}
