namespace ELearning.Core.DTOs.Submission;

public record SubmissionResponseDto(
    Guid Id, 
    Guid LessonId, 
    Guid ClassId, 
    Guid StudentId, 
    string? SubmissionUrl, 
    string? StudentNote, 
    DateTime SubmittedAt, 
    float? Score, 
    string? Feedback
);

// Dành cho Sinh viên nộp bài
public record CreateSubmissionRequestDto(
    Guid LessonId, 
    Guid ClassId, // Lấy từ URL khi sinh viên đang vào học lớp đó
    Guid StudentId, 
    string? SubmissionUrl, 
    string? StudentNote
);

// Dành cho Giảng viên chấm điểm
public record GradeSubmissionRequestDto(
    float Score, 
    string? Feedback
);