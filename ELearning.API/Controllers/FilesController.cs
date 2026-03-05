using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        try
        {
            // Có thể truyền folderId của bạn vào tham số thứ 2 nếu muốn
            var fileLink = await _driveService.UploadFileAsync(file); 
            
            return Ok(new 
            { 
                message = "Upload thành công", 
                url = fileLink 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi upload file", error = ex.Message });
        }
    }
}