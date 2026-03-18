using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml; // EPPlus

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpPost("users/import")]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        // 1. Kiểm tra file có tồn tại và đúng định dạng không
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn một file Excel hợp lệ!" });

        if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
            return BadRequest(new { message = "Chỉ hỗ trợ định dạng .xls hoặc .xlsx" });

        // Khai báo bản quyền sử dụng EPPlus (Bắt buộc từ bản 5.0 trở đi)
        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");

        var importedUsers = new List<string>(); // Danh sách tạm để chứa tên người dùng vừa import

        // 2. Mở file ra đọc
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                // Lấy cái Sheet đầu tiên (Sheet1)
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                // Vòng lặp đọc từ dòng số 2 (Bỏ qua dòng 1 là Tiêu đề cột)
                for (int row = 2; row <= rowCount; row++)
                {
                    // Lấy dữ liệu từng cột (Giả sử Cột 1: Tên, Cột 2: Email, Cột 3: Pass)
                    var fullName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var password = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(email)) continue; // Bỏ qua dòng trống

                    // TODO: Tại đây, bạn sẽ gọi _authService.RegisterAsync(...) để lưu vào DB
                    // var isSuccess = await _authService.RegisterAsync(new RegisterRequestDto(email, password, fullName, "Student"));
                    
                    importedUsers.Add(fullName ?? email);
                }
            }
        }

        return Ok(new { 
            message = $"Đã nhập thành công {importedUsers.Count} tài khoản!",
            users = importedUsers 
        });
    }
}