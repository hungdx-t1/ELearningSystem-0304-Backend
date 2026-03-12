using Microsoft.AspNetCore.Http;

namespace ELearning.Core.Interfaces.Services;
            
[Obsolete("Google Drive integration is deprecated. Consider using Cloudinary for file uploads instead.")]
public interface IGoogleDriveService
{
    // Nhận file từ Frontend (IFormFile) và trả về cái Link Google Drive
    Task<string> UploadFileAsync(IFormFile file, string? folderId = null);
    
    // (Tuỳ chọn) Hàm xóa file nếu người dùng muốn up lại
    Task<bool> DeleteFileAsync(string fileId);
}