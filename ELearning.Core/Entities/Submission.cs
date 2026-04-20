namespace ELearning.Core.Entities;

// lớp này dùng để lưu thông tin bài nộp của sinh viên cho mỗi bài học, bao gồm cả bài tập tự luận và trắc nghiệm
public class Submission
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public string? SubmissionUrl { get; set; } // Link file sinh viên nộp (Cloudinary) cho tự luận
    public string? StudentNote { get; set; } // Nội dung chữ sinh viên gõ cho phần tự luận
    public string? QuizAnswersJson { get; set; }  // Lưu đáp án trắc nghiệm dưới dạng chuỗi JSON: {"cau_1_id": "A", "cau_2_id": "C"}
    public int CheatWarnings { get; set; } = 0; // Lưu số lần Cảnh báo gian lận (Chuyển tab, thoát toàn màn hình)
    public DateTime? StartedAt { get; set; } // Thời điểm bắt đầu làm bài (để tính thời gian)
    public bool IsSubmitted { get; set; } = false; // Đánh dấu bài thi đã nộp hay chưa (nếu đang thi mà rớt mạng thì IsSubmitted = false)
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public float? Score { get; set; }
    public string? Feedback { get; set; }
}