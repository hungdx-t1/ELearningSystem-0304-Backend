using ELearning.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace ELearning.Services.Implements;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public AiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> ChatWithAiAsync(string userMessage)
    {
        var apiKey = _config["GeminiAI:ApiKey"];
        var url = $"{_config["GeminiAI:Url"]}?key={apiKey}";

        // 1. Đóng gói dữ liệu theo chuẩn cấu trúc của Google Gemini
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = userMessage } } }
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // 2. Gửi request sang Google
        var response = await _httpClient.PostAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            return "Xin lỗi, hiện tại não bộ AI của tôi đang bảo trì. Bạn vui lòng thử lại sau nhé!";
        }

        // 3. Đọc dữ liệu trả về và bóc lớp lấy đúng câu chữ
        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);

        try
        {
            // Bóc tách JSON: candidates[0] -> content -> parts[0] -> text
            var reply = jsonDocument.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return reply ?? "Mình không hiểu ý bạn lắm.";
        }
        catch
        {
            return "Lỗi khi giải mã phản hồi từ AI.";
        }
    }

    public async Task<string> GenerateQuizAsync(string topic, int questionCount)
    {
        var apiKey = _config["GeminiAI:ApiKey"];
        var url = $"{_config["GeminiAI:Url"]}?key={apiKey}";

        // 1. Viết Prompt ép AI làm giáo viên và trả về đúng schema
        string prompt = $@"
            Bạn là một chuyên gia giáo dục. Hãy tạo {questionCount} câu hỏi trắc nghiệm về chủ đề '{topic}'.
            BẮT BUỘC phải trả về đúng định dạng mảng JSON sau, KHÔNG kèm theo bất kỳ văn bản nào khác, KHÔNG dùng markdown ```json:
            [
              {{
                ""content"": ""Nội dung câu hỏi"",
                ""optionA"": ""Đáp án A"",
                ""optionB"": ""Đáp án B"",
                ""optionC"": ""Đáp án C"",
                ""optionD"": ""Đáp án D"",
                ""correctOption"": ""A"", // Chỉ ghi đúng 1 chữ cái A, B, C hoặc D
                ""explanation"": ""Giải thích ngắn gọn lý do chọn đáp án này""
              }}
            ]
        ";

        // 2. Cấu hình generationConfig ép kiểu application/json
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new 
            { 
                response_mime_type = "application/json" 
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, jsonContent);
        
        if (!response.IsSuccessStatusCode)
        {
            return "[]"; // Trả về mảng rỗng nếu gọi API thất bại
        }

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);
        
        try
        {
            var reply = jsonDocument.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return reply ?? "[]";
        }
        catch
        {
            return "[]";
        }
    }

    public async Task<string> GenerateQuizFromFileAsync(IFormFile file, string topic, int questionCount)
    {
        var apiKey = _config["GeminiAI:ApiKey"];
        var url = $"{_config["GeminiAI:Url"]}?key={apiKey}";

        // 1. Chuyển file thành mảng Byte rồi ép sang Base64
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var base64File = Convert.ToBase64String(memoryStream.ToArray());
        var mimeType = file.ContentType; // Ví dụ: application/pdf

        // 2. Viết Prompt yêu cầu AI đọc tài liệu đính kèm
        string topicInstruction = string.IsNullOrWhiteSpace(topic) 
            ? "toàn bộ nội dung tài liệu đính kèm" 
            : $"chủ đề '{topic}' dựa trên tài liệu đính kèm";

        string prompt = $@"
            Bạn là một chuyên gia giáo dục. Hãy ĐỌC KỸ TÀI LIỆU ĐÍNH KÈM và tạo {questionCount} câu hỏi trắc nghiệm tập trung vào {topicInstruction}.
            BẮT BUỘC phải trả về đúng định dạng mảng JSON sau, KHÔNG kèm theo bất kỳ văn bản nào khác:
            [
              {{
                ""content"": ""Nội dung câu hỏi"",
                ""optionA"": ""Đáp án A"",
                ""optionB"": ""Đáp án B"",
                ""optionC"": ""Đáp án C"",
                ""optionD"": ""Đáp án D"",
                ""correctOption"": ""A"", 
                ""explanation"": ""Giải thích ngắn gọn lý do""
              }}
            ]
        ";

        // 3. Đóng gói dữ liệu Đa phương thức (Text + File)
        var requestBody = new
        {
            contents = new[]
            {
                new 
                { 
                    parts = new object[] 
                    { 
                        new { text = prompt },
                        new 
                        { 
                            inline_data = new 
                            {
                                mime_type = mimeType,
                                data = base64File
                            }
                        }
                    } 
                }
            },
            generationConfig = new { response_mime_type = "application/json" }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, jsonContent);
        
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