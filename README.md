# Hệ Thống E-Learning (Backend)

Đây là mã nguồn Backend cho hệ thống học trực tuyến (E-Learning System) được xây dựng với kiến trúc **Layered Architecture** hiện đại.

## 🚀 Công Nghệ Sử Dụng (Tech Stack)
- **Framework:** ASP.NET Core (.NET 10.0)
- **Ngôn ngữ:** C# 13
- **Cơ Sở Dữ Liệu:** PostgreSQL
- **ORM:** Entity Framework Core
- **Tài liệu API:** OpenAPI với giao diện Scalar
- **Xác thực:** JWT (JSON Web Tokens) Bearer Authentication
- **External Services:** Cloudinary (Quản lý file/media)

## 📁 Cấu Trúc Thư Mục (Architecture)
Dự án được ứng dụng mô hình Multiple Projects (Kiến trúc phân tầng) trong Solution `ELearningSystem.slnx`:

1. **`ELearning.Core`**: Lớp chứa các thành phần cốt lõi của hệ thống (Entities, Enums, DTOs, Interfaces...). Nó hoàn toàn độc lập và không phụ thuộc vào bất cứ package nào khác.
2. **`ELearning.Infrastructure`**: Lớp giao tiếp trực tiếp với cơ sở hạ tầng thực tế. Nó bao gồm `AppDbContext` (EF Core) để tương tác PostgreSQL, các Repositories Pattern thực thi, và các dịch vụ ngoài (VD: Cloudinary upload ảnh/video).
3. **`ELearning.Services`**: Lớp xử lý nghiệp vụ (Business Logic). Nhận dữ liệu thông qua Repository từ Db và trả lại kết quả (hoặc DTO) cho API Controller.
4. **`ELearning.API`**: Lớp trình diễn (Web API). Đây là nơi định nghĩa các endpoint (Controllers), nhận request HTTP từ Web/App Frontend, chạy các middlewares, authorization, CORS và gọi xuống lớp Services.

## 🌟 Chức Năng Chính (Features)
-  **Quản lý Khóa học & Lớp học:** Quản lý khóa học (Courses), nhóm thành các chương (Chapters) và bài học (Lessons - Hỗ trợ Video).
-  **Bài Tập & Chấm Điểm:** Tính năng tạo bài tập cho lớp học và học viên có thể nộp bài tập tự luận (Submissions).
-  **Bộ câu hỏi (Question Bank):** Quản lý chung các câu hỏi trắc nghiệm/tự luận.
-  **Xác Thực & Phân Quyền:** Có 3 Role quyền (Admin, Instructor, Student).
-  **Tích hợp AI Chat:** Hỗ trợ tính năng trò chuyện, giải đáp của AI tạo sinh, lưu lại nhật ký Chat (AI Chat Logs).

## ⚡ Hướng Dẫn Chạy Dự Án
1. Đảm bảo bạn đã cài đặt **.NET 10.0 SDK** và **PostgreSQL**.
2. Thiết lập lại cấu hình Db Connection, JWT Key và thông tin Cloudinary ở file `ELearning.API/appsettings.json`.
3. Chạy lệnh Migration (hoặc ứng dụng sẽ tự chạy Entity Framework migrations tự động nhờ đoạn cấu hình có trong `Program.cs` khi start dự án).
4. Sử dụng lệnh để chạy API:
   ```bash
   cd ELearning.API
   dotnet run
   ```
5. Mở Postman hoặc duyệt vào đường dẫn Scalar sinh ra mặc định của `.NET` để xem danh sách API Endpoint.
