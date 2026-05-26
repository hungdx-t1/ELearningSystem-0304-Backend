using ELearning.Services.Implements;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace ELearning.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _emailService = new EmailService(_mockConfig.Object);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrowException_WhenSenderEmailIsMissing()
    {
        // Arrange
        _mockConfig.Setup(c => c["SmtpConfig:SenderName"]).Returns("LMS");
        _mockConfig.Setup(c => c["SmtpConfig:SenderEmail"]).Returns((string)null!);
        _mockConfig.Setup(c => c["SmtpConfig:Password"]).Returns("password123");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _emailService.SendEmailAsync("test@example.com", "Subject", "Body"));
            
        Assert.Equal("SmtpConfig is not properly configured in appsettings.json", exception.Message);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrowException_WhenPasswordIsMissing()
    {
        // Arrange
        _mockConfig.Setup(c => c["SmtpConfig:SenderName"]).Returns("LMS");
        _mockConfig.Setup(c => c["SmtpConfig:SenderEmail"]).Returns("admin@example.com");
        _mockConfig.Setup(c => c["SmtpConfig:Password"]).Returns((string)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _emailService.SendEmailAsync("test@example.com", "Subject", "Body"));
            
        Assert.Equal("SmtpConfig is not properly configured in appsettings.json", exception.Message);
    }
}
