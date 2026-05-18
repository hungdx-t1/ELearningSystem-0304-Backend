using ELearning.Core.DTOs.Admin; 
using ELearning.Core.DTOs.Student;

namespace ELearning.Core.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetDashboardSummaryAsync();
    Task<StudentDashboardResponseDto> GetStudentDashboardAsync(Guid studentId);
}