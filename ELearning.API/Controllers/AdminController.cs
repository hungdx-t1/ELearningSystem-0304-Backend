using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(IUserService userService) : ControllerBase
{
    [HttpPost("users/import")]
    [EndpointSummary("Nhập dữ liệu các user từ file Excel")]
    [EndpointDescription("Xử lý file Excel được tải lên và import dữ liệu các user vào hệ thống.")]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn một file Excel hợp lệ!" });

        if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
            return BadRequest(new { message = "Chỉ hỗ trợ định dạng .xls hoặc .xlsx" });

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var result = await userService.ImportUsersFromExcelAsync(stream);

        return Ok(new
        {
            message = $"Đã nhập thành công {result.SuccessCount} tài khoản!",
            successCount = result.SuccessCount,
            errors = result.Errors
        });
    }

    [HttpGet("users/export")]
    [EndpointSummary("Xuất dữ liệu user ra file Excel")]
    [EndpointDescription("Xuất toàn bộ dữ liệu user ra một file Excel (.xlsx).")]
    public async Task<IActionResult> ExportUsers()
    {
        var fileBytes = await userService.ExportUsersToExcelAsync();
        string excelName = "DanhSachTaiKhoan.xlsx";

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
    }
}