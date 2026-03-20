using ELearning.Core.DTOs.Admin;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    // Tiêm thẳng DbContext vào đây cho nhanh, vì Dashboard thường query phức tạp nhiều bảng
    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponseDto> GetDashboardSummaryAsync()
    {
        // 1. TÍNH TOÁN KPI
        var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        var activeCourses = await _context.Courses.CountAsync();
        var runningClasses = await _context.Classes.CountAsync();
        
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
            var count = await _context.ClassEnrollments
                .Where(e => e.EnrollmentDate.Month == targetMonth.Month && e.EnrollmentDate.Year == targetMonth.Year)
                .CountAsync();
                
            chartData.Add(new ChartDataDto($"Tháng {targetMonth.Month}", count));
        }

        // 3. HOẠT ĐỘNG GẦN ĐÂY
        var recentClasses = await _context.Classes
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
}