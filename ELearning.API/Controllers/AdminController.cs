using ELearning.Core.DTOs.User;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml; // EPPlus

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("users/import")]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn một file Excel hợp lệ!" });

        if (!file.FileName.EndsWith(".xls") && !file.FileName.EndsWith(".xlsx"))
            return BadRequest(new { message = "Chỉ hỗ trợ định dạng .xls hoặc .xlsx" });

        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");

        var importedUsers = new List<string>();
        var errorRows = new List<string>(); // Danh sách chứa các dòng bị lỗi để báo cáo

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            using (var package = new ExcelPackage(stream))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                // Đọc từ dòng 2 (Bỏ qua header)
                for (int row = 2; row <= rowCount; row++)
                {
                    var fullName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var email = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var password = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                    var roleString = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                    var adminClass = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

                    // Bỏ qua nếu dòng đó trống Tên hoặc Email
                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName)) 
                        continue; 

                    try
                    {
                        // 1. PHIÊN DỊCH VAI TRÒ (Từ chữ trong Excel sang Enum)
                        UserRole role = UserRole.Student; // Mặc định là Học viên
                        if (!string.IsNullOrEmpty(roleString))
                        {
                            var r = roleString.ToLower();
                            if (r.Contains("admin") || r.Contains("quản trị")) role = UserRole.Admin;
                            else if (r.Contains("instructor") || r.Contains("giảng viên")) role = UserRole.Instructor;
                        }

                        // 2. SINH MÃ USER CODE (Vì file Excel không có nhưng DTO lại bắt buộc)
                        string prefix = role == UserRole.Student ? "STU" : (role == UserRole.Instructor ? "INS" : "ADM");
                        string randomSuffix = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
                        string userCode = $"{prefix}-{DateTime.Now:yyMM}-{randomSuffix}";

                        // 3. XỬ LÝ MẬT KHẨU MẶC ĐỊNH (Nếu Excel không điền pass)
                        string finalPassword = string.IsNullOrEmpty(password) ? "Default@123" : password;

                        // 4. GÓI DỮ LIỆU VÀO DTO (Lưu ý: dùng đúng thứ tự khai báo của record)
                        var requestDto = new CreateUserRequestDto(
                            userCode,
                            fullName,
                            email,
                            finalPassword,
                            role,
                            adminClass
                        );

                        // 5. GỌI SERVICE LƯU XUỐNG DB
                        await _userService.CreateUserAsync(requestDto);
                        
                        importedUsers.Add(email);
                    }
                    catch (Exception ex)
                    {
                        // Nếu dòng này bị lỗi (ví dụ: trùng email), ghi nhận lại và chạy tiếp dòng sau
                        errorRows.Add($"Dòng {row} ({email}): {ex.Message}");
                    }
                }
            }
        }

        // Trả về báo cáo chi tiết
        return Ok(new { 
            message = $"Đã nhập thành công {importedUsers.Count} tài khoản!",
            successCount = importedUsers.Count,
            errors = errorRows 
        });
    }
    
    [HttpGet("users/export")]
    public async Task<IActionResult> ExportUsers()
    {
        ExcelPackage.License.SetNonCommercialPersonal("LMS Project");

        var users = await _userService.GetAllUsersAsync();
        var userList = users.ToList(); // Ép sang List để dễ đếm index trong vòng lặp

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("DanhSachNguoiDung");

        // 3. TẠO THANH TIÊU ĐỀ (HEADER) - Chuẩn theo UserResponseDto
        string[] headers = { "STT", "Mã Định Danh", "Họ và Tên", "Email", "Vai trò", "Lớp hành chính", "Trạng thái", "Ngày tham gia" };
        
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
        }

        // Tô màu xám và in đậm cho Header
        using (var range = worksheet.Cells[1, 1, 1, headers.Length])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        // 4. ĐỔ DỮ LIỆU THẬT VÀO TỪNG DÒNG
        for (int i = 0; i < userList.Count; i++)
        {
            var user = userList[i];
            var row = i + 2; // Bắt đầu ghi từ dòng số 2

            // Phiên dịch Role Enum sang tiếng Việt cho Sếp dễ đọc
            string roleName = user.Role switch
            {
                UserRole.Admin => "Quản trị viên",
                UserRole.Instructor => "Giảng viên",
                UserRole.Student => "Học viên",
                _ => "Chưa xác định"
            };

            // Điền dữ liệu
            worksheet.Cells[row, 1].Value = i + 1; // Số thứ tự tự tăng
            worksheet.Cells[row, 2].Value = user.UserCode;
            worksheet.Cells[row, 3].Value = user.FullName;
            worksheet.Cells[row, 4].Value = user.Email;
            worksheet.Cells[row, 5].Value = roleName; // Chữ đã được dịch
            worksheet.Cells[row, 6].Value = user.AdministrativeClass ?? "Chưa xếp lớp"; // Nếu null thì báo chưa xếp
            worksheet.Cells[row, 7].Value = user.IsActive ? "Hoạt động" : "Khóa";
            worksheet.Cells[row, 8].Value = user.CreatedAt.ToString("dd/MM/yyyy HH:mm"); // Format ngày tháng cho đẹp
        }

        // Tự động căn chỉnh độ rộng các cột sao cho không bị che mất chữ
        worksheet.Cells.AutoFitColumns();

        // 5. ĐÓNG GÓI VÀ GỬI VỀ FRONTEND
        var stream = new MemoryStream();
        await package.SaveAsAsync(stream); // Dùng Async lưu file cho mượt Server
        stream.Position = 0; 

        // Tên file có kèm thời gian thực để tải nhiều lần không bị trùng tên
        string excelName = $"DanhSachNguoiDung_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
    }
}