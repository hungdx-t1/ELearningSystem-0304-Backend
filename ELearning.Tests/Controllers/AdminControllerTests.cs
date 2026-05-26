using ELearning.API.Controllers;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ELearning.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _controller = new AdminController(_mockUserService.Object);
    }

    [Fact]
    public async Task ImportUsers_ShouldReturnBadRequest_WhenFileIsNull()
    {
        // Act
        var result = await _controller.ImportUsers(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Vui lòng chọn một file Excel hợp lệ", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task ImportUsers_ShouldReturnBadRequest_WhenFileExtensionIsInvalid()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(100);
        mockFile.Setup(f => f.FileName).Returns("test.txt");

        // Act
        var result = await _controller.ImportUsers(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Chỉ hỗ trợ định dạng .xls hoặc .xlsx", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task ImportUsers_ShouldReturnOk_WhenSuccessful()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(100);
        mockFile.Setup(f => f.FileName).Returns("test.xlsx");
        
        _mockUserService.Setup(s => s.ImportUsersFromExcelAsync(It.IsAny<Stream>()))
            .ReturnsAsync((10, new List<string>()));

        // Act
        var result = await _controller.ImportUsers(mockFile.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Đã nhập thành công 10 tài khoản", okResult.Value?.ToString());
        _mockUserService.Verify(s => s.ImportUsersFromExcelAsync(It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public async Task ExportUsers_ShouldReturnFileResult()
    {
        // Arrange
        var dummyBytes = new byte[] { 1, 2, 3 };
        _mockUserService.Setup(s => s.ExportUsersToExcelAsync()).ReturnsAsync(dummyBytes);

        // Act
        var result = await _controller.ExportUsers();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Equal("DanhSachTaiKhoan.xlsx", fileResult.FileDownloadName);
        Assert.Equal(dummyBytes, fileResult.FileContents);
    }
}
