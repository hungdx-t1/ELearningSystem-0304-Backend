using System.Net;
using ELearning.Services.Implements;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace ELearning.Tests.Services;

public class AiServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly AiService _aiService;

    public AiServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["GeminiAI:ApiKey"]).Returns("test-api-key");
        _mockConfig.Setup(c => c["GeminiAI:Url"]).Returns("https://test-gemini-url.com");

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _aiService = new AiService(_httpClient, _mockConfig.Object, null!);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ShouldReturnVector_WhenApiSucceeds()
    {
        // Arrange
        var jsonResponse = @"{ ""embedding"": { ""values"": [0.1, 0.2, 0.3] } }";
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _aiService.GenerateEmbeddingAsync("Hello");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.ToArray().Length);
        Assert.Equal(0.1f, result.ToArray()[0]);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ShouldReturnEmptyVector_WhenApiFails()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        // Act
        var result = await _aiService.GenerateEmbeddingAsync("Hello");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(768, result.ToArray().Length); // Returns empty vector of 768 zeros
    }

    [Fact]
    public async Task GenerateQuizAsync_ShouldReturnJsonArray_WhenApiSucceeds()
    {
        // Arrange
        var expectedJson = @"[{ ""content"": ""Q1"", ""correctOption"": ""A"" }]";
        
        var geminiResponse = $@"{{
            ""candidates"": [
                {{
                    ""content"": {{
                        ""parts"": [
                            {{ ""text"": ""{expectedJson.Replace("\"", "\\\"")}"" }}
                        ]
                    }}
                }}
            ]
        }}";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(geminiResponse)
            });

        // Act
        var result = await _aiService.GenerateQuizAsync("C# Basic", 1, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Q1", result);
    }
}
