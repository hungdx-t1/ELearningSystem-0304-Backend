using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class AiService(HttpClient httpClient, IConfiguration config, AppDbContext context) : IAiService
{
    public async Task<Pgvector.Vector> GenerateEmbeddingAsync(string text)
    {
        var apiKey = config["GeminiAI:ApiKey"];
        var embedUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent?key={apiKey}";

        var requestBody = new
        {
            model = "models/gemini-embedding-001",
            content = new
            {
                parts = new[] { new { text = text } }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(embedUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            return new Pgvector.Vector(new float[768]);
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);
        var values = jsonDocument.RootElement.GetProperty("embedding").GetProperty("values").EnumerateArray().Select(x => x.GetSingle()).ToArray();

        return new Pgvector.Vector(values);
    }

    public async Task<string> ChatWithAiAsync(string userMessage, List<Guid>? lessonIds, IFormFile? file = null, string? similarContext = null, string? userName = null)
    {
        var apiKey = config["GeminiAI:ApiKey"];
        var geminiUrl = $"{config["GeminiAI:Url"]}?key={apiKey}";

        // Rổ chứa dữ liệu gửi lên Gemini (bắt đầu bằng câu hỏi text của user)
        var parts = new List<object> { new { text = userMessage } };

        string systemInstructionText = "Bạn là một trợ lý ảo giáo dục trên hệ thống LMS. Nhiệm vụ của bạn là giải đáp thắc mắc liên quan đến học thuật. TỪ CHỐI mọi câu hỏi ngoài luồng.";

        bool hasContext = false; // có cung cấp tài liệu (context) hay không

        if (!string.IsNullOrWhiteSpace(similarContext))
        {
            hasContext = true;
        }

        // nếu có file từ người dùng có tự upload file đính kèm
        if (file != null && file.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            parts.Add(new
            {
                inline_data = new { mime_type = file.ContentType, data = Convert.ToBase64String(memoryStream.ToArray()) }
            });
            hasContext = true;
        }

        // nếu có file từ người dùng chọn từ danh sách bài học
        if (lessonIds != null && lessonIds.Any())
        {
            var lessons = await context.Lessons
                .Where(l => lessonIds.Contains(l.Id) && l.DocumentUrl != null)
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                try
                {
                    // Tải file PDF từ Cloudinary của từng bài học
                    var fileResponse = await httpClient.GetAsync(lesson.DocumentUrl);
                    if (fileResponse.IsSuccessStatusCode)
                    {
                        var fileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
                        var mimeType = fileResponse.Content.Headers.ContentType?.MediaType ?? "application/pdf";

                        // Nhét tiếp vào rổ
                        parts.Add(new
                        {
                            inline_data = new { mime_type = mimeType, data = Convert.ToBase64String(fileBytes) }
                        });
                        hasContext = true;
                    }
                }
                catch { /* Bỏ qua nếu lỗi mạng tải file bài học */ }
            }
        }

        // prompt nếu có dính dáng tới TÀI LIỆU
        if (hasContext)
        {
            systemInstructionText = @"Bạn là trợ lý ảo giải đáp bài giảng. 
                QUY TẮC TỐI THƯỢNG: 
                1. Ưu tiên sử dụng thông tin có trong các tài liệu đính kèm hoặc lịch sử chat trước đó (nếu có) để trả lời.
                2. Nếu thông tin không có trong tài liệu/lịch sử, bạn có thể dùng kiến thức bên ngoài nhưng phải nói rõ: 'Theo tài liệu thì không đề cập, nhưng theo kiến thức chung thì...'";

            if (!string.IsNullOrWhiteSpace(similarContext))
            {
                systemInstructionText += $"\n\nLỊCH SỬ CHAT TRƯỚC ĐÓ CỦA HỌC VIÊN:\n{similarContext}";
            }
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            systemInstructionText = $"Người đang nói chuyện với bạn là một học viên có tên: {userName}. Bạn hãy xưng hô thân thiện bằng tên của họ trong câu trả lời nhé.\n\n" + systemInstructionText;
        }

        // Đóng gói tất cả vào request
        var requestBody = new
        {
            system_instruction = new { parts = new[] { new { text = systemInstructionText } } },
            contents = new[] { new { parts = parts.ToArray() } }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(geminiUrl, jsonContent);

        if (!response.IsSuccessStatusCode)
            return "Xin lỗi, hiện tại não bộ AI của tôi đang bảo trì. Bạn vui lòng thử lại sau nhé!";

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);

        try
        {
            return jsonDocument.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "Mình không hiểu ý bạn lắm.";
        }
        catch { return "Lỗi khi giải mã phản hồi từ AI."; }
    }

    public async Task<string> GenerateQuizAsync(string topic, int questionCount, List<Guid>? lessonIds, IFormFile? file = null)
    {
        var apiKey = config["GeminiAI:ApiKey"];
        var geminiUrl = $"{config["GeminiAI:Url"]}?key={apiKey}";

        bool hasContext = false;
        var parts = new List<object>();

        if (file != null && file.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            parts.Add(new
            {
                inline_data = new { mime_type = file.ContentType, data = Convert.ToBase64String(memoryStream.ToArray()) }
            });
            hasContext = true;
        }

        if (lessonIds != null && lessonIds.Any())
        {
            var lessons = await context.Lessons
                .Where(l => lessonIds.Contains(l.Id) && l.DocumentUrl != null)
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                try
                {
                    var fileResponse = await httpClient.GetAsync(lesson.DocumentUrl);
                    if (fileResponse.IsSuccessStatusCode)
                    {
                        var fileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
                        var mimeType = fileResponse.Content.Headers.ContentType?.MediaType ?? "application/pdf";
                        parts.Add(new
                        {
                            inline_data = new { mime_type = mimeType, data = Convert.ToBase64String(fileBytes) }
                        });
                        hasContext = true;
                    }
                }
                catch { /* Bỏ qua nếu lỗi mạng */ }
            }
        }

        // CHUẨN BỊ PROMPT THEO NGỮ CẢNH (CÓ HAY KHÔNG CÓ FILE)
        string promptText = "";

        if (hasContext)
        {
            promptText = $@"
            Bạn là một chuyên gia giáo dục. Hãy ĐỌC KỸ CÁC TÀI LIỆU ĐÍNH KÈM và tạo {questionCount} câu hỏi trắc nghiệm tập trung vào chủ đề '{topic}'.
            
            CÁC QUY TẮC BẮT BUỘC:
            1. CHỈ lấy dữ kiện từ trong tài liệu đính kèm để tạo câu hỏi và đáp án.
            2. TUYỆT ĐỐI KHÔNG dùng kiến thức bên ngoài, không tự sáng tác thêm nội dung.
            3. Nếu tài liệu quá ngắn, hãy chỉ tạo số lượng câu hỏi tối đa có thể (không bịa thêm).";
        }
        else
        {
            promptText = $@"
            Bạn là một chuyên gia giáo dục. Hãy tạo {questionCount} câu hỏi trắc nghiệm về chủ đề '{topic}'.";
        }

        // Thêm yêu cầu về Format JSON cho cả 2 trường hợp
        promptText += @"
            BẮT BUỘC trả về đúng định dạng mảng JSON sau, KHÔNG kèm theo bất kỳ văn bản nào khác:
            [
              {
                ""content"": ""Nội dung câu hỏi"",
                ""optionA"": ""Đáp án A"",
                ""optionB"": ""Đáp án B"",
                ""optionC"": ""Đáp án C"",
                ""optionD"": ""Đáp án D"",
                ""correctOption"": ""A"", 
                ""explanation"": ""Giải thích ngắn gọn lý do""
              }
            ]";

        // Nhét câu lệnh Prompt vào ĐẦU giỏ dữ liệu
        parts.Insert(0, new { text = promptText });

        // GỌI API GEMINI
        var requestBody = new
        {
            contents = new[] { new { parts = parts.ToArray() } },
            generationConfig = new { response_mime_type = "application/json" }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(geminiUrl, jsonContent);

        if (!response.IsSuccessStatusCode) return "[]";

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);

        try
        {
            return jsonDocument.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "[]";
        }
        catch { return "[]"; }
    }
}