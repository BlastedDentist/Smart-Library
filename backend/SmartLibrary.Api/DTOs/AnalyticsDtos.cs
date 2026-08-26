namespace SmartLibrary.Api.DTOs;

public class OccupancyPointDto
{
    public string Label { get; set; } = string.Empty; // e.g. "Mon", "2026-07-10", "09:00"
    public int VisitCount { get; set; }
}

public class HourlyLoadDto
{
    public int Hour { get; set; } // 0-23
    public int VisitCount { get; set; }
}

public class AnalyticsSummaryDto
{
    public List<OccupancyPointDto> DailyOccupancy { get; set; } = new();
    public List<OccupancyPointDto> WeeklyOccupancy { get; set; } = new();
    public List<OccupancyPointDto> MonthlyOccupancy { get; set; } = new();
    public List<HourlyLoadDto> HourlyLoad { get; set; } = new();
    public List<int> PeakHours { get; set; } = new();
    public List<int> QuietHours { get; set; } = new();
    public double AverageVisitDurationMinutes { get; set; }
    public string BestTimeToVisit { get; set; } = string.Empty;
}
