using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearning.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController(ICloudinaryService cloudinaryService) : ControllerBase
{
    [HttpPost("upload")]
    [EndpointSummary("Tải file lên")]
    [EndpointDescription("Upload tài liệu hoặc hình ảnh lên hệ thống.")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File rỗng!");

        try
        {
            var fileUrl = await cloudinaryService.UploadFileAsync(file);
            return Ok(new { message = "Upload thành công!", url = fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi Upload", error = ex.Message });
        }
    }
}