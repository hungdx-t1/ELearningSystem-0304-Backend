using ELearning.Core.Interfaces.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;
using File = Google.Apis.Drive.v3.Data.File;

namespace ELearning.Infrastructure.ExternalServices;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly string _credentialsFilePath = "google_credentials.json"; // Tên file key của bạn
    private readonly string _applicationName = "LMS Elearning App";

    private DriveService GetDriveService()
    {
        // Đọc file JSON credential được tải từ Google Cloud Console
        GoogleCredential credential;
        using (var stream = new FileStream(_credentialsFilePath, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.Drive);
        }

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName,
        });
    }

    public async Task<string> UploadFileAsync(IFormFile file, string? folderId = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File không hợp lệ hoặc rỗng");

        var service = GetDriveService();

        var fileMetadata = new File
        {
            Name = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{file.FileName}",
        };

        // Nếu bạn muốn lưu vào 1 folder cụ thể trên Drive (Ví dụ: Folder "BaiTapSinhVien")
        if (!string.IsNullOrEmpty(folderId))
        {
            fileMetadata.Parents = new List<string> { folderId };
        }

        // Upload file
        FilesResource.CreateMediaUpload request;
        using (var stream = file.OpenReadStream())
        {
            request = service.Files.Create(fileMetadata, stream, file.ContentType);
            request.Fields = "id, webViewLink, webContentLink"; // Lấy link sau khi up xong
            await request.UploadAsync();
        }

        var uploadedFile = request.ResponseBody;

        // BƯỚC QUAN TRỌNG: Cấp quyền "Anyone with the link can view"
        // Để Frontend Angular/Flutter có thể load được video/ảnh mà không cần bắt sinh viên đăng nhập Google
        var permission = new Google.Apis.Drive.v3.Data.Permission
        {
            Type = "anyone",
            Role = "reader"
        };
        await service.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

        // webViewLink: Mở ra giao diện xem (để nhúng PDF, Video)
        // webContentLink: Link tải trực tiếp
        return uploadedFile.WebViewLink; 
    }

    public async Task<bool> DeleteFileAsync(string fileId)
    {
        try
        {
            var service = GetDriveService();
            await service.Files.Delete(fileId).ExecuteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}