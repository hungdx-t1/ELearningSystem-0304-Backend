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
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy chi tiết dashboard (Admin)")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất thông tin chi tiết của dashboard dành cho Quản trị viên.")]
    public async Task<IActionResult> GetDashboardData()
    {
        var result = await dashboardService.GetDashboardSummaryAsync();
        return Ok(result);
    }

    [HttpGet("student")]
    [Microsoft.AspNetCore.Http.EndpointSummary("Lấy chi tiết dashboard (Học viên)")]
    [Microsoft.AspNetCore.Http.EndpointDescription("Truy xuất thông tin thống kê và lớp học dành cho Học viên.")]
    public async Task<IActionResult> GetStudentDashboardData()
    {
        Guid studentId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await dashboardService.GetStudentDashboardAsync(studentId);
        return Ok(result);
    }
}