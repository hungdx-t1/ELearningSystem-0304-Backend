using ELearning.Core.Interfaces.Services;
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
}