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

                    // TODO: Tại đây gọi _authService.RegisterAsync(...) để lưu vào DB
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

    [HttpGet("users/export")]
    public IActionResult ExportUsers()
    {
        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");

        // Tạm thời giả lập danh sách User (Sau này bạn gọi từ DB ra bằng Entity Framework)
        var users = new List<dynamic>
        {
            new { FullName = "Nguyễn Văn A", Email = "nguyenvana@gmail.com", Role = "Student", Status = "Hoạt động" },
            new { FullName = "Trần Thị B", Email = "tranthib@gmail.com", Role = "Instructor", Status = "Hoạt động" },
            new { FullName = "Quản trị viên C", Email = "admin@lms.com", Role = "Admin", Status = "Khóa" }
        };

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("DanhSachNguoiDung");

        // 1. Tạo thanh Tiêu đề (Header)
        worksheet.Cells[1, 1].Value = "STT";
        worksheet.Cells[1, 2].Value = "Họ và Tên";
        worksheet.Cells[1, 3].Value = "Email";
        worksheet.Cells[1, 4].Value = "Vai trò";
        worksheet.Cells[1, 5].Value = "Trạng thái";

        // Tô màu xám và in đậm cho Header trông cho chuyên nghiệp
        using (var range = worksheet.Cells[1, 1, 1, 5])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // 2. Đổ dữ liệu từ danh sách vào các dòng
        for (int i = 0; i < users.Count; i++)
        {
            var row = i + 2; // Dòng 1 là header rồi nên data bắt đầu từ dòng 2
            worksheet.Cells[row, 1].Value = i + 1;
            worksheet.Cells[row, 2].Value = users[i].FullName;
            worksheet.Cells[row, 3].Value = users[i].Email;
            worksheet.Cells[row, 4].Value = users[i].Role;
            worksheet.Cells[row, 5].Value = users[i].Status;
        }

        // Tự động căn chỉnh độ rộng cột cho đẹp, không bị chèn chữ
        worksheet.Cells.AutoFitColumns();

        // 3. Đóng gói thành file và gửi về
        var stream = new MemoryStream();
        package.SaveAs(stream);
        stream.Position = 0; // Trả con trỏ về đầu stream để người nhận đọc được

        string excelName = $"DanhSachNguoiDung_{DateTime.Now:yyyyMMdd}.xlsx";
        
        // Trả về file với định dạng chuẩn của Excel
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
    }
}