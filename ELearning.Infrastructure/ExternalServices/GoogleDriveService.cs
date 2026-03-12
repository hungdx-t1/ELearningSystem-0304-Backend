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

        if (!string.IsNullOrEmpty(folderId))
        {
            fileMetadata.Parents = new List<string> { folderId };
        }

        FilesResource.CreateMediaUpload request;
        using (var stream = file.OpenReadStream())
        {
            // 1. Xử lý an toàn nếu file (.jar, .zip...) không có ContentType
            var contentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;

            request = service.Files.Create(fileMetadata, stream, contentType);
            request.Fields = "id, webViewLink, webContentLink";

            // 2. Chờ upload và LẤY TRẠNG THÁI TIẾN TRÌNH
            var progress = await request.UploadAsync();

            // 3. KIỂM TRA LỖI TỪ GOOGLE
            if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
            {
                throw new Exception($"Lỗi từ Google Drive: {progress.Exception?.Message}");
            }
        }

        var uploadedFile = request.ResponseBody;

        // Đề phòng Google giở chứng không trả về file
        if (uploadedFile == null)
        {
            throw new Exception("Upload thành công nhưng Google không trả về thông tin file.");
        }

        // 4. Phân quyền xem công khai
        var permission = new Google.Apis.Drive.v3.Data.Permission
        {
            Type = "anyone",
            Role = "reader"
        };
        await service.Permissions.Create(permission, uploadedFile.Id).ExecuteAsync();

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