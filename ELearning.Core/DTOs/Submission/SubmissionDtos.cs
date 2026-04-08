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
    string? Feedback,
    string? QuizAnswersJson, 
    int CheatWarnings,       
    bool IsSubmitted         
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

// Dành cho Sinh viên nộp bài Trắc nghiệm (Chỉ cần gửi Điểm)
// 08/04: Nâng cấp Request nộp bài Trắc nghiệm để hỗ trợ Auto-save (Lưu nháp) và lưu đáp án trắc nghiệm dưới dạng JSON, cũng như số lần vi phạm gian lận
public record SubmitQuizRequestDto(
    Guid LessonId, 
    Guid ClassId, 
    Guid StudentId, 
    float? Score,             // Cho phép null vì lúc Auto-save chưa có điểm
    string? QuizAnswersJson,  // Truyền chuỗi JSON đáp án lên
    int CheatWarnings,        // Số lần vi phạm
    bool IsSubmitted          // True: Nộp bài thật, False: Lưu nháp (Auto-save)
);