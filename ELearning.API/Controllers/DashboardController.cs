using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy chi tiết dashboard")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất thông tin chi tiết của một dashboard cụ thể thông qua ID.")]
    public async Task<IActionResult> GetDashboardData()
    {
        var result = await dashboardService.GetDashboardSummaryAsync();
        return Ok(result);
    }
}