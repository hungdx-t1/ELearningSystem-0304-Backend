using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Core.DTOs.Submission;

namespace ELearning.Services.Implements;

public class SubmissionService : ISubmissionService
{
    private readonly IGenericRepository<Submission> _submissionRepo;

    public SubmissionService(IGenericRepository<Submission> submissionRepo)
    {
        _submissionRepo = submissionRepo;
    }

    public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync(Guid classId, Guid lessonId)
    {
        var submissions = await _submissionRepo.FindAsync(s => s.ClassId == classId && s.LessonId == lessonId);
        
        return submissions.Select(s => new SubmissionResponseDto(
            s.Id, s.LessonId, s.ClassId, s.StudentId, s.SubmissionUrl, s.StudentNote, s.SubmittedAt, s.Score, s.Feedback));
    }

    public async Task<SubmissionResponseDto?> GetSubmissionAsync(Guid classId, Guid lessonId, Guid studentId)
    {
        var subs = await _submissionRepo.FindAsync(s => s.ClassId == classId && s.LessonId == lessonId && s.StudentId == studentId);
        var s = subs.FirstOrDefault();
        
        if (s == null) return null;
        
        return new SubmissionResponseDto(s.Id, s.LessonId, s.ClassId, s.StudentId, s.SubmissionUrl, s.StudentNote, s.SubmittedAt, s.Score, s.Feedback);
    }

    public async Task<SubmissionResponseDto> SubmitWorkAsync(CreateSubmissionRequestDto request)
    {
        // Kiểm tra xem sinh viên đã nộp bài trước đó chưa (Nếu có thì ghi đè/cập nhật)
        var existingSubs = await _submissionRepo.FindAsync(s => s.ClassId == request.ClassId && s.LessonId == request.LessonId && s.StudentId == request.StudentId);
        var existingSub = existingSubs.FirstOrDefault();

        if (existingSub != null)
        {
            existingSub.SubmissionUrl = request.SubmissionUrl;
            existingSub.StudentNote = request.StudentNote;
            existingSub.SubmittedAt = DateTime.UtcNow; // Cập nhật giờ nộp mới
            
            _submissionRepo.Update(existingSub);
            await _submissionRepo.SaveChangesAsync();
            
            return new SubmissionResponseDto(existingSub.Id, existingSub.LessonId, existingSub.ClassId, existingSub.StudentId, existingSub.SubmissionUrl, existingSub.StudentNote, existingSub.SubmittedAt, existingSub.Score, existingSub.Feedback);
        }

        // Nếu chưa nộp thì tạo mới
        var newSub = new Submission
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            ClassId = request.ClassId,
            StudentId = request.StudentId,
            SubmissionUrl = request.SubmissionUrl,
            StudentNote = request.StudentNote,
            SubmittedAt = DateTime.UtcNow
        };
        
        await _submissionRepo.AddAsync(newSub);
        await _submissionRepo.SaveChangesAsync();

        return new SubmissionResponseDto(newSub.Id, newSub.LessonId, newSub.ClassId, newSub.StudentId, newSub.SubmissionUrl, newSub.StudentNote, newSub.SubmittedAt, newSub.Score, newSub.Feedback);
    }

    public async Task<bool> GradeSubmissionAsync(Guid id, GradeSubmissionRequestDto request)
    {
        var sub = await _submissionRepo.GetByIdAsync(id);
        if (sub == null) return false;

        sub.Score = request.Score;
        sub.Feedback = request.Feedback;
        
        _submissionRepo.Update(sub);
        return await _submissionRepo.SaveChangesAsync();
    }
}