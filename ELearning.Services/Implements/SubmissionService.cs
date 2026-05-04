using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Core.DTOs.Submission;
using ClosedXML.Excel;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class SubmissionService(AppDbContext context, IGenericRepository<Submission> submissionRepo) : ISubmissionService
{
    public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync(Guid classId, Guid lessonId)
    {
        var submissions = await submissionRepo.FindAsync(s => s.ClassId == classId && s.LessonId == lessonId);

        return submissions.Select(s => new SubmissionResponseDto(
            s.Id, s.LessonId, s.ClassId, s.StudentId, s.SubmissionUrl, s.StudentNote, s.SubmittedAt, s.Score, s.Feedback, s.QuizAnswersJson, s.CheatWarnings, s.IsSubmitted, s.StartedAt));
    }

    public async Task<SubmissionResponseDto?> GetSubmissionAsync(Guid classId, Guid lessonId, Guid studentId)
    {
        var subs = await submissionRepo.FindAsync(s => s.ClassId == classId && s.LessonId == lessonId && s.StudentId == studentId);
        var s = subs.FirstOrDefault();

        if (s == null) return null;

        return new SubmissionResponseDto(s.Id, s.LessonId, s.ClassId, s.StudentId, s.SubmissionUrl, s.StudentNote, s.SubmittedAt, s.Score, s.Feedback, s.QuizAnswersJson, s.CheatWarnings, s.IsSubmitted, s.StartedAt);
    }

    public async Task<SubmissionResponseDto> StartExamAsync(Guid classId, Guid lessonId, Guid studentId)
    {
        var subs = await submissionRepo.FindAsync(s => s.ClassId == classId && s.LessonId == lessonId && s.StudentId == studentId);
        var existingSub = subs.FirstOrDefault();

        // Nếu đã có vết thì chỉ cần trả lại (để tránh F5 làm reset thời gian)
        if (existingSub != null)
        {
            if (!existingSub.StartedAt.HasValue)
            {
                existingSub.StartedAt = DateTime.UtcNow;
                submissionRepo.Update(existingSub);
                await submissionRepo.SaveChangesAsync();
            }
            return new SubmissionResponseDto(existingSub.Id, existingSub.LessonId, existingSub.ClassId, existingSub.StudentId, existingSub.SubmissionUrl, existingSub.StudentNote, existingSub.SubmittedAt, existingSub.Score, existingSub.Feedback, existingSub.QuizAnswersJson, existingSub.CheatWarnings, existingSub.IsSubmitted, existingSub.StartedAt);
        }

        // Nếu chưa bao giờ làm, thì khởi tạo bản ghi trước
        var newSub = new Submission
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            ClassId = classId,
            StudentId = studentId,
            StartedAt = DateTime.UtcNow,
            IsSubmitted = false
        };

        await submissionRepo.AddAsync(newSub);
        await submissionRepo.SaveChangesAsync();

        return new SubmissionResponseDto(newSub.Id, newSub.LessonId, newSub.ClassId, newSub.StudentId, newSub.SubmissionUrl, newSub.StudentNote, newSub.SubmittedAt, newSub.Score, newSub.Feedback, newSub.QuizAnswersJson, newSub.CheatWarnings, newSub.IsSubmitted, newSub.StartedAt);
    }

    public async Task<SubmissionResponseDto> SubmitWorkAsync(CreateSubmissionRequestDto request)
    {
        var existingSubs = await submissionRepo.FindAsync(s => s.ClassId == request.ClassId && s.LessonId == request.LessonId && s.StudentId == request.StudentId);
        var existingSub = existingSubs.FirstOrDefault();

        if (existingSub != null)
        {
            existingSub.SubmissionUrl = request.SubmissionUrl;
            existingSub.StudentNote = request.StudentNote;
            existingSub.SubmittedAt = DateTime.UtcNow;
            existingSub.IsSubmitted = true; // 🌟 Nộp tự luận thì auto là True

            submissionRepo.Update(existingSub);
            await submissionRepo.SaveChangesAsync();

            return new SubmissionResponseDto(existingSub.Id, existingSub.LessonId, existingSub.ClassId, existingSub.StudentId, existingSub.SubmissionUrl, existingSub.StudentNote, existingSub.SubmittedAt, existingSub.Score, existingSub.Feedback, existingSub.QuizAnswersJson, existingSub.CheatWarnings, existingSub.IsSubmitted, existingSub.StartedAt);
        }

        var newSub = new Submission
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            ClassId = request.ClassId,
            StudentId = request.StudentId,
            SubmissionUrl = request.SubmissionUrl,
            StudentNote = request.StudentNote,
            SubmittedAt = DateTime.UtcNow,
            IsSubmitted = true // 🌟
        };

        await submissionRepo.AddAsync(newSub);
        await submissionRepo.SaveChangesAsync();

        return new SubmissionResponseDto(newSub.Id, newSub.LessonId, newSub.ClassId, newSub.StudentId, newSub.SubmissionUrl, newSub.StudentNote, newSub.SubmittedAt, newSub.Score, newSub.Feedback, newSub.QuizAnswersJson, newSub.CheatWarnings, newSub.IsSubmitted, newSub.StartedAt);
    }

    public async Task<bool> GradeSubmissionAsync(Guid id, GradeSubmissionRequestDto request)
    {
        var sub = await submissionRepo.GetByIdAsync(id);
        if (sub == null) return false;

        sub.Score = request.Score;
        sub.Feedback = request.Feedback;

        submissionRepo.Update(sub);
        return await submissionRepo.SaveChangesAsync();
    }

    public async Task<SubmissionResponseDto> SubmitQuizAsync(SubmitQuizRequestDto request)
    {
        var existingSubs = await submissionRepo.FindAsync(s => s.ClassId == request.ClassId && s.LessonId == request.LessonId && s.StudentId == request.StudentId);
        var existingSub = existingSubs.FirstOrDefault();

        if (existingSub != null)
        {
            // Chỉ cập nhật điểm nếu request có gửi điểm lên (Tránh đè mất điểm khi lưu nháp)
            if (request.Score.HasValue) existingSub.Score = request.Score.Value;

            existingSub.QuizAnswersJson = request.QuizAnswersJson;
            existingSub.CheatWarnings = request.CheatWarnings;
            existingSub.IsSubmitted = request.IsSubmitted;
            existingSub.SubmittedAt = DateTime.UtcNow;

            submissionRepo.Update(existingSub);
            await submissionRepo.SaveChangesAsync();

            return new SubmissionResponseDto(existingSub.Id, existingSub.LessonId, existingSub.ClassId, existingSub.StudentId, existingSub.SubmissionUrl, existingSub.StudentNote, existingSub.SubmittedAt, existingSub.Score, existingSub.Feedback, existingSub.QuizAnswersJson, existingSub.CheatWarnings, existingSub.IsSubmitted, existingSub.StartedAt);
        }

        var newSub = new Submission
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            ClassId = request.ClassId,
            StudentId = request.StudentId,
            Score = request.Score,
            QuizAnswersJson = request.QuizAnswersJson,
            CheatWarnings = request.CheatWarnings,
            IsSubmitted = request.IsSubmitted,
            SubmittedAt = DateTime.UtcNow
        };

        await submissionRepo.AddAsync(newSub);
        await submissionRepo.SaveChangesAsync();

        return new SubmissionResponseDto(newSub.Id, newSub.LessonId, newSub.ClassId, newSub.StudentId, newSub.SubmissionUrl, newSub.StudentNote, newSub.SubmittedAt, newSub.Score, newSub.Feedback, newSub.QuizAnswersJson, newSub.CheatWarnings, newSub.IsSubmitted, newSub.StartedAt);
    }

    public async Task<byte[]> ExportScoresToExcelAsync(Guid lessonId)
    {
        // (join) nối bảng Submissions và Users qua cột StudentId
        var query = from s in context.Submissions
                    join u in context.Users on s.StudentId equals u.Id
                    where s.LessonId == lessonId
                    orderby s.SubmittedAt // Sắp xếp theo thời gian nộp
                    select new
                    {
                        s.Score,
                        s.SubmittedAt,
                        s.Feedback,
                        StudentName = u.FullName ?? "Chưa cập nhật tên",
                        StudentCode = u.UserCode ?? u.Email ?? "Không rõ mã SV"
                    };

        var submissionList = await query.ToListAsync();

        // Khởi tạo file Excel
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Bảng Điểm");

        // Header + trang trí (Cập nhật lại tiêu đề cột cho chuẩn)
        var headers = new[] { "STT", "Họ và Tên", "Mã Sinh Viên", "Điểm Số", "Thời Gian Nộp", "Trạng Thái", "Nhận Xét" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.Teal; // Đổi màu Header
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Đổ dữ liệu đã được Join vào từng dòng
        for (int i = 0; i < submissionList.Count; i++)
        {
            var sub = submissionList[i];
            int row = i + 2;

            worksheet.Cell(row, 1).Value = i + 1;
            worksheet.Cell(row, 2).Value = sub.StudentName;
            worksheet.Cell(row, 3).Value = sub.StudentCode;
            worksheet.Cell(row, 4).Value = sub.Score.HasValue ? sub.Score.Value.ToString() : "Chưa chấm";
            worksheet.Cell(row, 5).Value = sub.SubmittedAt.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(row, 6).Value = sub.Score.HasValue ? "Đã chấm" : "Chờ chấm";

            // Tô màu nhẹ cho cột Trạng thái để Giảng viên dễ nhìn
            if (sub.Score.HasValue)
            {
                worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Green;
            }
            else
            {
                worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Orange;
            }

            worksheet.Cell(row, 7).Value = sub.Feedback ?? "";
        }

        // Tự động căn chỉnh độ rộng cột cho vừa nội dung chữ
        worksheet.Columns().AdjustToContents();

        // 5. Chuyển file Excel thành mảng Byte
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}