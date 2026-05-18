using ELearning.Core.DTOs.Admin;
using ELearning.Core.DTOs.Student;
using ELearning.Core.DTOs.Course;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class DashboardService(AppDbContext context, ICourseService courseService) : IDashboardService
{
    public async Task<DashboardResponseDto> GetDashboardSummaryAsync()
    {
        // 1. TÍNH TOÁN KPI
        var totalStudents = await context.Users.CountAsync(u => u.Role == UserRole.Student);
        var activeCourses = await context.Courses.CountAsync();
        var runningClasses = await context.Classes.CountAsync();

        var kpis = new List<KpiDto>
        {
            new("Tổng Học viên", totalStudents.ToString(), "+12%", true, "👨‍🎓", "bg-blue-50 text-blue-600"),
            new("Khóa học", activeCourses.ToString(), "+2", true, "📚", "bg-indigo-50 text-indigo-600"),
            new("Lớp học", runningClasses.ToString(), "Ổn định", true, "🏫", "bg-emerald-50 text-emerald-600"),
            new("Doanh thu", "0 đ", "Hệ thống nội bộ", true, "💰", "bg-rose-50 text-rose-600")
        };

        // 2. DỮ LIỆU BIỂU ĐỒ
        var chartData = new List<ChartDataDto>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = DateTime.UtcNow.AddMonths(-i);
            var count = await context.ClassEnrollments
                .Where(e => e.EnrollmentDate.Month == targetMonth.Month && e.EnrollmentDate.Year == targetMonth.Year)
                .CountAsync();

            chartData.Add(new ChartDataDto($"Tháng {targetMonth.Month}", count));
        }

        // 3. HOẠT ĐỘNG GẦN ĐÂY
        var recentClasses = await context.Classes
            .Include(c => c.Course)
            .OrderByDescending(c => c.Id)
            .Take(3)
            .ToListAsync();

        var activities = recentClasses.Select(c => new ActivityDto(
            "Hệ thống", "vừa mở lớp học mới", c.ClassCode, "Gần đây"
        )).ToList();

        // Trả món ăn đã hoàn thiện ra ngoài
        return new DashboardResponseDto(kpis, chartData, activities);
    }

    public async Task<StudentDashboardResponseDto> GetStudentDashboardAsync(Guid studentId)
    {
        var allCourses = await courseService.GetAllCoursesAsync();

        var myClasses = await context.ClassEnrollments
            .Include(e => e.Class)
            .ThenInclude(c => c.Course)
            .Where(e => e.StudentId == studentId)
            .Select(e => new StudentClassDto(
                e.Class.Id,
                e.Class.CourseId,
                e.Class.ClassCode,
                e.Class.ClassName,
                e.Class.Course != null ? e.Class.Course.Title : "Chưa rõ môn",
                e.Class.AcademicYear,
                e.Class.GoogleMeetLink
            ))
            .ToListAsync();

        var submissions = await context.Submissions
            .Where(s => s.StudentId == studentId && s.IsSubmitted)
            .ToListAsync();

        int completedCount = submissions.Count;
        var scores = submissions.Where(s => s.Score.HasValue).Select(s => s.Score!.Value).ToList();
        float averageScore = scores.Any() ? (float)Math.Round(scores.Average(), 1) : 0f;

        return new StudentDashboardResponseDto(allCourses.ToList(), myClasses, completedCount, averageScore);
    }
}