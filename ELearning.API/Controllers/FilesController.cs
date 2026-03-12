using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] //TODO tạm thời bỏ auth để test, khi nào ghép frontend thì bật lại
public class FilesController : ControllerBase
{
    private readonly IGoogleDriveService _driveService;

    public FilesController(IGoogleDriveService driveService)
    {
        _driveService = driveService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        // Kiểm tra file rỗng
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn một file hợp lệ!" });

        // Kiểm tra dung lượng (Ví dụ: Giới hạn 50MB = 50 * 1024 * 1024 bytes)
        var maxFileSize = 50 * 1024 * 1024; 
        if (file.Length > maxFileSize)
            return BadRequest(new { message = "Dung lượng file quá lớn. Vui lòng chọn file dưới 50MB." });

        try
        {
            // Gọi Service đẩy thẳng lên Google Drive
            // Nếu bạn đã tạo folder trên Drive, có thể truyền ID folder vào tham số thứ 2
            var fileUrl = await _driveService.UploadFileAsync(file, "1EldoFPGUKb4Hei2HIRa42Gz9y_CbgiWE"); 
            
            // Trả về kết quả cho Angular/Flutter
            return Ok(new 
            { 
                message = "Upload thành công!", 
                url = fileUrl,
                fileName = file.FileName,
                size = file.Length
            });
        }
        catch (Exception ex)
        {
            // Nếu báo lỗi, thường là do file google_credentials.json chưa đúng
            return StatusCode(500, new { message = "Lỗi khi giao tiếp với Google Drive", error = ex.Message });
        }
    }
}