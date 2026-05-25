using ELearning.Core.DTOs.User;
using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces;
using ELearning.Infrastructure.Data;
using ELearning.Services.Implements;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ELearning.Tests.Services;

public class UserServiceTests : IDisposable
{
    private readonly Mock<IGenericRepository<User>> _mockUserRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IGenericRepository<User>>();
        _userService = new UserService(_mockUserRepository.Object, null!);
    }

    public void Dispose()
    {
    }


    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var expectedUsers = new List<User>
        {
            new User { Id = Guid.NewGuid(), FullName = "User 1", Email = "user1@example.com", Role = UserRole.Student },
            new User { Id = Guid.NewGuid(), FullName = "User 2", Email = "user2@example.com", Role = UserRole.Instructor }
        };

        _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User { Id = userId, FullName = "Test User", Email = "test@example.com" };
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("Test User", result.FullName);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnNewUser_AndHashPassword()
    {
        // Arrange
        var request = new CreateUserRequestDto("STU-1234", "New User", "new@gmail.com", "Password@123", UserRole.Student, "Class A");
        
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New User", result.FullName);
        Assert.Equal("new@gmail.com", result.Email);
        _mockUserRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == request.Email && !string.IsNullOrEmpty(u.PasswordHash) && u.PasswordHash != request.Password)), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnTrue_WhenUpdateIsSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, FullName = "Old Name" };
        var request = new UpdateUserRequestDto("New Name", "http://avatar.url", DateTime.UtcNow, "Class B", false);

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUserAsync(userId, request);

        // Assert
        Assert.True(result);
        Assert.Equal("New Name", existingUser.FullName);
        Assert.Equal("Class B", existingUser.AdministrativeClass);
        Assert.False(existingUser.IsActive);
        _mockUserRepository.Verify(r => r.Update(existingUser), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequestDto("New Name", null, null, null, true);
        
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null!);

        // Act
        var result = await _userService.UpdateUserAsync(userId, request);

        // Assert
        Assert.False(result);
        _mockUserRepository.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnTrue_WhenDeleteIsSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.Delete(existingUser));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUserAsync(userId);

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(r => r.Delete(existingUser), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldThrowInvalidOperationException_WhenDbUpdateExceptionOccurs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.Delete(existingUser));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new DbUpdateException());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.DeleteUserAsync(userId));
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ShouldToggleStatusAndReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, IsActive = true };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.Update(It.IsAny<User>()));
        _mockUserRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _userService.ToggleUserStatusAsync(userId);

        // Assert
        Assert.True(result);
        Assert.False(existingUser.IsActive); // Should be toggled to false
        _mockUserRepository.Verify(r => r.Update(existingUser), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
