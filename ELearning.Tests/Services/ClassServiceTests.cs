using ELearning.Core.DTOs.Class;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace ELearning.Tests.Services;

public class ClassServiceTests
{
    private readonly Mock<IGenericRepository<Class>> _mockClassRepo;
    private readonly Mock<IGenericRepository<ClassEnrollment>> _mockEnrollmentRepo;
    private readonly ClassService _classService;

    public ClassServiceTests()
    {
        _mockClassRepo = new Mock<IGenericRepository<Class>>();
        _mockEnrollmentRepo = new Mock<IGenericRepository<ClassEnrollment>>();
        
        // Pass null for AppDbContext because we only test methods that use generic repositories
        _classService = new ClassService(_mockClassRepo.Object, _mockEnrollmentRepo.Object, null!);
    }

    [Fact]
    public async Task CreateClassAsync_ShouldCreateNewClass_WhenCodeDoesNotExist()
    {
        // Arrange
        var request = new CreateClassRequestDto(Guid.NewGuid(), "IT101", "IT Fundamentals", Guid.NewGuid(), "http://meet", "2024-2025", "Desc");
        
        _mockClassRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Class, bool>>>()))
                      .ReturnsAsync(new List<Class>());
                      
        _mockClassRepo.Setup(r => r.AddAsync(It.IsAny<Class>())).Returns(Task.CompletedTask);
        _mockClassRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _classService.CreateClassAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("IT101", result.ClassCode);
        Assert.Equal("IT Fundamentals", result.ClassName);
        _mockClassRepo.Verify(r => r.AddAsync(It.IsAny<Class>()), Times.Once);
        _mockClassRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateClassAsync_ShouldThrowException_WhenCodeAlreadyExists()
    {
        // Arrange
        var request = new CreateClassRequestDto(Guid.NewGuid(), "IT101", "IT Fundamentals", Guid.NewGuid(), "http://meet", "2024-2025", "Desc");
        
        _mockClassRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Class, bool>>>()))
                      .ReturnsAsync(new List<Class> { new Class { Id = Guid.NewGuid(), ClassCode = "IT101" } });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _classService.CreateClassAsync(request));
        Assert.Contains("Mã lớp học đã tồn tại", ex.Message);
        _mockClassRepo.Verify(r => r.AddAsync(It.IsAny<Class>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClassAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var request = new UpdateClassRequestDto(Guid.NewGuid(), "IT102", "Advanced IT", Guid.NewGuid(), "http://meet", "2024-2025", "Desc");
        var existingClass = new Class { Id = classId, ClassCode = "IT101" };

        _mockClassRepo.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);
        _mockClassRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Class, bool>>>()))
                      .ReturnsAsync(new List<Class>()); // No duplicate code
                      
        _mockClassRepo.Setup(r => r.Update(It.IsAny<Class>()));
        _mockClassRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _classService.UpdateClassAsync(classId, request);

        // Assert
        Assert.True(result);
        Assert.Equal("IT102", existingClass.ClassCode);
        Assert.Equal("Advanced IT", existingClass.ClassName);
        _mockClassRepo.Verify(r => r.Update(existingClass), Times.Once);
        _mockClassRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateClassAsync_ShouldThrowException_WhenDuplicateCodeExists()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var request = new UpdateClassRequestDto(Guid.NewGuid(), "IT102", "Advanced IT", Guid.NewGuid(), "http://meet", "2024-2025", "Desc");
        var existingClass = new Class { Id = classId, ClassCode = "IT101" };

        _mockClassRepo.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);
        _mockClassRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Class, bool>>>()))
                      .ReturnsAsync(new List<Class> { new Class { Id = Guid.NewGuid(), ClassCode = "IT102" } }); // Duplicate code

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _classService.UpdateClassAsync(classId, request));
        Assert.Contains("Mã lớp học đã tồn tại", ex.Message);
        _mockClassRepo.Verify(r => r.Update(It.IsAny<Class>()), Times.Never);
    }

    [Fact]
    public async Task DeleteClassAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var existingClass = new Class { Id = classId };

        _mockClassRepo.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync(existingClass);
        _mockClassRepo.Setup(r => r.Delete(existingClass));
        _mockClassRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _classService.DeleteClassAsync(classId);

        // Assert
        Assert.True(result);
        _mockClassRepo.Verify(r => r.Delete(existingClass), Times.Once);
        _mockClassRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteClassAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        var classId = Guid.NewGuid();
        _mockClassRepo.Setup(r => r.GetByIdAsync(classId)).ReturnsAsync((Class)null!);

        // Act
        var result = await _classService.DeleteClassAsync(classId);

        // Assert
        Assert.False(result);
        _mockClassRepo.Verify(r => r.Delete(It.IsAny<Class>()), Times.Never);
    }

    [Fact]
    public async Task EnrollStudentAsync_ShouldReturnTrue_WhenNotEnrolled()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        
        _mockEnrollmentRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClassEnrollment, bool>>>()))
                           .ReturnsAsync(new List<ClassEnrollment>());
                           
        _mockEnrollmentRepo.Setup(r => r.AddAsync(It.IsAny<ClassEnrollment>())).Returns(Task.CompletedTask);
        _mockEnrollmentRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _classService.EnrollStudentAsync(classId, studentId);

        // Assert
        Assert.True(result);
        _mockEnrollmentRepo.Verify(r => r.AddAsync(It.IsAny<ClassEnrollment>()), Times.Once);
    }

    [Fact]
    public async Task EnrollStudentAsync_ShouldReturnTrue_WhenAlreadyEnrolled()
    {
        // Arrange
        var classId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        
        _mockEnrollmentRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ClassEnrollment, bool>>>()))
                           .ReturnsAsync(new List<ClassEnrollment> { new ClassEnrollment() });

        // Act
        var result = await _classService.EnrollStudentAsync(classId, studentId);

        // Assert
        Assert.True(result);
        _mockEnrollmentRepo.Verify(r => r.AddAsync(It.IsAny<ClassEnrollment>()), Times.Never);
    }
}
