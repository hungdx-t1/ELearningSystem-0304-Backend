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

    public async Task<string> ChatWithAiAsync(string userMessage, IFormFile? file = null)
    {
        var apiKey = _config["GeminiAI:ApiKey"];
        var url = $"{_config["GeminiAI:Url"]}?key={apiKey}";

        // 1. Khởi tạo danh sách các thành phần (parts) của câu hỏi
        var parts = new List<object>
        {
            new { text = userMessage }
        };

        string systemInstructionText = "Bạn là một trợ lý ảo giáo dục trên hệ thống LMS. Nhiệm vụ của bạn là giải đáp thắc mắc liên quan đến học thuật. TỪ CHỐI mọi câu hỏi ngoài luồng.";

        // 2. NẾU CÓ FILE ĐÍNH KÈM -> Chuyển thành Base64 và nhét thêm vào danh sách parts
        if (file != null && file.Length > 0)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var base64File = Convert.ToBase64String(memoryStream.ToArray());

            parts.Add(new
            {
                inline_data = new
                {
                    mime_type = file.ContentType,
                    data = base64File
                }
            });

            systemInstructionText = @"Bạn là trợ lý ảo giải đáp bài giảng. 
                QUY TẮC TỐI THƯỢNG: 
                1. CHỈ ĐƯỢC PHÉP sử dụng thông tin có trong tài liệu đính kèm để trả lời. 
                2. TUYỆT ĐỐI KHÔNG sử dụng kiến thức bên ngoài, không tự bịa đặt hay suy diễn thêm dữ kiện.
                3. Nếu sinh viên hỏi thông tin KHÔNG CÓ trong tài liệu, bạn PHẢI TRẢ LỜI chính xác câu này: 'Xin lỗi, thông tin bạn hỏi không được đề cập trong bài học/tài liệu này.'";
        }

        // 3. Đóng gói dữ liệu kèm theo SYSTEM INSTRUCTION
        var requestBody = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new
                    {
                        text = systemInstructionText
                    }
                }
            },
            contents = new[]
            {
                new { parts = parts.ToArray() } // Truyền danh sách parts (có thể có hoặc không có file) vào đây
            }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)
            return "Xin lỗi, hiện tại não bộ AI của tôi đang bảo trì. Bạn vui lòng thử lại sau nhé!";

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
            
            CÁC QUY TẮC BẮT BUỘC PHẢI TUÂN THỦ NGHIÊM NGẶT:
            1. CHỈ lấy dữ kiện từ trong tài liệu đính kèm để tạo câu hỏi và đáp án.
            2. TUYỆT ĐỐI KHÔNG dùng kiến thức bên ngoài, không tự sáng tác thêm nội dung không có trong file.
            3. Nếu tài liệu quá ngắn, không đủ thông tin để tạo đủ {questionCount} câu, hãy chỉ tạo số lượng câu hỏi tối đa mà tài liệu cho phép, không được bịa thêm cho đủ số lượng.
            
            BẮT BUỘC trả về đúng định dạng mảng JSON sau, KHÔNG kèm theo bất kỳ văn bản nào khác:
            [
              {{
                ""content"": ""Nội dung câu hỏi"",
                ""optionA"": ""Đáp án A"",
                ""optionB"": ""Đáp án B"",
                ""optionC"": ""Đáp án C"",
                ""optionD"": ""Đáp án D"",
                ""correctOption"": ""A"", 
                ""explanation"": ""Giải thích ngắn gọn lý do dựa theo tài liệu""
              }}
            ]
        ";

        // 3. Đóng gói dữ liệu Đa phương thức (Text + File)
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new object[] { new { text = prompt }, new { inline_data = new { mime_type = mimeType, data = base64File } } } }
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