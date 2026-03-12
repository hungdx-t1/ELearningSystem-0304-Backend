using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ELearning.Infrastructure.ExternalServices;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        // Lấy thông tin từ appsettings.json để mở khóa
        var account = new Account(
            configuration["CloudinarySettings:CloudName"],
            configuration["CloudinarySettings:ApiKey"],
            configuration["CloudinarySettings:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName = "LMS_Uploads")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File không hợp lệ hoặc rỗng");

        using var stream = file.OpenReadStream();
        
        // Nhận diện loại file để Cloudinary xử lý tối ưu nhất
        var isImage = file.ContentType.StartsWith("image/");
        var isVideo = file.ContentType.StartsWith("video/");

        UploadParams uploadParams;

        if (isImage)
        {
            uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };
        }
        else if (isVideo)
        {
            uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };
        }
        else
        {
            // Dành cho PDF, DOCX, ZIP, JAR...
            uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderName
            };
        }

        // Bắn lên mây
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception($"Lỗi từ Cloudinary: {uploadResult.Error.Message}");
        }

        // Trả về cái Link an toàn (HTTPS)
        return uploadResult.SecureUrl.ToString();
    }
}