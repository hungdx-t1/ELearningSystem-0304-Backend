namespace ELearning.Core.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public string? SubmissionUrl { get; set; } // Link file sinh viên nộp (Cloudinary)
    public string? StudentNote { get; set; } // Nội dung chữ sinh viên gõ
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public float? Score { get; set; }
    public string? Feedback { get; set; }
}