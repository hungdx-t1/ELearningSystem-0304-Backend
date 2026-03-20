namespace ELearning.Core.DTOs.Admin;

public record KpiDto(string Title, string Value, string Trend, bool IsUp, string Icon, string Color);
public record ChartDataDto(string Label, int Value);
public record ActivityDto(string User, string Action, string Target, string Time);

public record DashboardResponseDto(
    List<KpiDto> Kpis,
    List<ChartDataDto> ChartData,
    List<ActivityDto> RecentActivities
);