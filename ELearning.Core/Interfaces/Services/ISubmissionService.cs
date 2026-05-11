using ELearning.Core.DTOs.Submission;

namespace ELearning.Core.Interfaces.Services;

public interface ISubmissionService
{
    // Lấy toàn bộ bài nộp của 1 Bài tập (Lesson) trong 1 Lớp học (Class) cụ thể
    Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync(Guid classId, Guid lessonId);

    // Lấy bài nộp của 1 Sinh viên cụ thể (để sinh viên tự xem lại bài của mình)
    Task<SubmissionResponseDto?> GetSubmissionAsync(Guid classId, Guid lessonId, Guid studentId);
    // API Khi SV lần đầu ấn vào bài thi để ghi vết StartedAt
    Task<SubmissionResponseDto> StartExamAsync(Guid classId, Guid lessonId, Guid studentId);

    Task<SubmissionResponseDto> SubmitWorkAsync(CreateSubmissionRequestDto request);
    Task<bool> GradeSubmissionAsync(Guid id, GradeSubmissionRequestDto request);
    Task<SubmissionResponseDto> SubmitQuizAsync(SubmitQuizRequestDto request);
    Task<byte[]> ExportScoresToExcelAsync(Guid lessonId);
    Task<IEnumerable<SubmissionHistoryDto>> GetStudentHistoryAsync(Guid studentId);
}