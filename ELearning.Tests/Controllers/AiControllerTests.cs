using System.Security.Claims;
using ELearning.API.Controllers;
using ELearning.Core.DTOs.AiChatLog;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ELearning.Tests.Controllers;

public class AiControllerTests
{
    private readonly Mock<IAiService> _mockAiService;
    private readonly Mock<IAiChatService> _mockAiChatService;
    private readonly Mock<IGenericRepository<User>> _mockUserRepo;
    private readonly AiController _controller;

    public AiControllerTests()
    {
        _mockAiService = new Mock<IAiService>();
        _mockAiChatService = new Mock<IAiChatService>();
        _mockUserRepo = new Mock<IGenericRepository<User>>();

        _controller = new AiController(_mockAiService.Object, _mockAiChatService.Object, _mockUserRepo.Object);
    }

    private void SetControllerUser(Guid userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Chat_ShouldReturnBadRequest_WhenPromptIsEmpty()
    {
        // Act
        var result = await _controller.Chat("", null, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Câu hỏi không được để trống", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Chat_ShouldReturnReply_WithContext_WhenUserIsAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetControllerUser(userId);

        var dbUser = new User { Id = userId, FullName = "Test User" };
        var similarChats = new List<AiChatLogDto>
        {
            new AiChatLogDto(Guid.NewGuid(), userId, "Hello", "Hi there", DateTime.UtcNow)
        };

        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(dbUser);
        _mockAiChatService.Setup(s => s.FindSimilarChatsAsync(userId, "How are you?", 3)).ReturnsAsync(similarChats);
        
        _mockAiService.Setup(s => s.ChatWithAiAsync("How are you?", null, null, It.Is<string>(c => c.Contains("Hello") && c.Contains("Hi there")), "Test User"))
            .ReturnsAsync("I am an AI");

        // Act
        var result = await _controller.Chat("How are you?", null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("I am an AI", okResult.Value?.ToString());
    }

    [Fact]
    public async Task Chat_ShouldReturnReply_WithoutContext_WhenUserNotAuthenticated()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // No claims
        };

        _mockAiService.Setup(s => s.ChatWithAiAsync("How are you?", null, null, null, null))
            .ReturnsAsync("I am an AI");

        // Act
        var result = await _controller.Chat("How are you?", null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("I am an AI", okResult.Value?.ToString());
    }

    [Fact]
    public async Task GenerateQuiz_ShouldReturnBadRequest_WhenQuestionCountIsInvalid()
    {
        // Act
        var result1 = await _controller.GenerateQuiz("Topic", 0, null, null);
        var result2 = await _controller.GenerateQuiz("Topic", 21, null, null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result1);
        Assert.IsType<BadRequestObjectResult>(result2);
    }

    [Fact]
    public async Task GenerateQuiz_ShouldReturnJsonArray_WhenSuccessful()
    {
        // Arrange
        var jsonResponse = @"[{ ""content"": ""Q1"" }]";
        _mockAiService.Setup(s => s.GenerateQuizAsync("Math", 5, null, null)).ReturnsAsync(jsonResponse);

        // Act
        var result = await _controller.GenerateQuiz("Math", 5, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GenerateQuiz_ShouldReturn500_WhenJsonIsInvalid()
    {
        // Arrange
        var invalidJson = @"[ invalid";
        _mockAiService.Setup(s => s.GenerateQuizAsync("Math", 5, null, null)).ReturnsAsync(invalidJson);

        // Act
        var result = await _controller.GenerateQuiz("Math", 5, null, null);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Contains("Lỗi khi xử lý dữ liệu từ AI", statusCodeResult.Value?.ToString());
    }
}
