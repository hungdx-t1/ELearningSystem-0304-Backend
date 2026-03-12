using Microsoft.AspNetCore.Http;

namespace ELearning.Core.Interfaces.Services;

public interface ICloudinaryService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName = "LMS_Uploads");
}