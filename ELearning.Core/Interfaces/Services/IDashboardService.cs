using ELearning.Core.DTOs.Admin; 

namespace ELearning.Core.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardResponseDto> GetDashboardSummaryAsync();
}