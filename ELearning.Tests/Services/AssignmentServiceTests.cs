using System.Linq.Expressions;
using ELearning.Core.DTOs.Assignment;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Services.Implements;
using Moq;
using Xunit;

namespace ELearning.Tests.Services;

public class AssignmentServiceTests
{
    private readonly Mock<IGenericRepository<Assignment>> _mockAssignmentRepo;
    private readonly AssignmentService _assignmentService;

    public AssignmentServiceTests()
    {
        _mockAssignmentRepo = new Mock<IGenericRepository<Assignment>>();
        _assignmentService = new AssignmentService(_mockAssignmentRepo.Object);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_ShouldReturnAllAssignments()
    {
        // Arrange
        var assignments = new List<Assignment>
        {
            new Assignment { Id = Guid.NewGuid(), Title = "A1" },
            new Assignment { Id = Guid.NewGuid(), Title = "A2" }
        };
        _mockAssignmentRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(assignments);

        // Act
        var result = await _assignmentService.GetAllAssignmentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_ShouldReturnAssignment_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var assignment = new Assignment { Id = id, Title = "A1" };
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(assignment);

        // Act
        var result = await _assignmentService.GetAssignmentByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task GetAssignmentByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Assignment)null!);

        // Act
        var result = await _assignmentService.GetAssignmentByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAssignmentAsync_ShouldReturnCreatedAssignment()
    {
        // Arrange
        var request = new CreateAssignmentRequestDto("New Assignment", "Description", DateTime.UtcNow.AddDays(7));
        _mockAssignmentRepo.Setup(r => r.AddAsync(It.IsAny<Assignment>())).Returns(Task.CompletedTask);
        _mockAssignmentRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _assignmentService.CreateAssignmentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Assignment", result.Title);
        _mockAssignmentRepo.Verify(r => r.AddAsync(It.IsAny<Assignment>()), Times.Once);
        _mockAssignmentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingAssignment = new Assignment { Id = id, Title = "Old Title" };
        var request = new UpdateAssignmentRequestDto("New Title", "New Desc", DateTime.UtcNow);

        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingAssignment);
        _mockAssignmentRepo.Setup(r => r.Update(existingAssignment));
        _mockAssignmentRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _assignmentService.UpdateAssignmentAsync(id, request);

        // Assert
        Assert.True(result);
        Assert.Equal("New Title", existingAssignment.Title);
        _mockAssignmentRepo.Verify(r => r.Update(existingAssignment), Times.Once);
        _mockAssignmentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAssignmentAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateAssignmentRequestDto("New Title", "New Desc", DateTime.UtcNow);
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Assignment)null!);

        // Act
        var result = await _assignmentService.UpdateAssignmentAsync(id, request);

        // Assert
        Assert.False(result);
        _mockAssignmentRepo.Verify(r => r.Update(It.IsAny<Assignment>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAssignmentAsync_ShouldReturnTrue_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingAssignment = new Assignment { Id = id };

        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingAssignment);
        _mockAssignmentRepo.Setup(r => r.Delete(existingAssignment));
        _mockAssignmentRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _assignmentService.DeleteAssignmentAsync(id);

        // Assert
        Assert.True(result);
        _mockAssignmentRepo.Verify(r => r.Delete(existingAssignment), Times.Once);
        _mockAssignmentRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAssignmentAsync_ShouldReturnFalse_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockAssignmentRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Assignment)null!);

        // Act
        var result = await _assignmentService.DeleteAssignmentAsync(id);

        // Assert
        Assert.False(result);
        _mockAssignmentRepo.Verify(r => r.Delete(It.IsAny<Assignment>()), Times.Never);
    }
}
