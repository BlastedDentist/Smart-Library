using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Models;
using SmartLibrary.Api.Repositories;

namespace SmartLibrary.Api.Services;

// Turns raw Attendance documents into the aggregated shapes the dashboard
// charts need. We deliberately compute this in C# (rather than a MongoDB
// aggregation pipeline) to keep the logic easy to read and adjust while
// you're learning - once the dataset grows large, this is exactly the kind
// of method you'd later migrate to a Mongo aggregation pipeline for
// performance. That upgrade path is a good exercise for later.
public class AnalyticsService : IAnalyticsService
{
    private readonly IAttendanceRepository _attendanceRepository;

    // "Quiet" and "peak" are relative to the busiest/least-busy hours seen in
    // the data, not absolute numbers - so the definition scales naturally
    // whether the library sees 20 visits a day or 2000.
    private const int TopHoursCount = 3;

    public AnalyticsService(IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync()
    {
        var all = await _attendanceRepository.GetAllAsync();

        return new AnalyticsSummaryDto
        {
            DailyOccupancy = BuildDailyOccupancy(all),
            WeeklyOccupancy = BuildWeeklyOccupancy(all),
            MonthlyOccupancy = BuildMonthlyOccupancy(all),
            HourlyLoad = BuildHourlyLoad(all, out var peakHours, out var quietHours),
            PeakHours = peakHours,
            QuietHours = quietHours,
            AverageVisitDurationMinutes = BuildAverageDuration(all),
            BestTimeToVisit = BuildBestTimeToVisitMessage(quietHours)
        };
    }

    // Last 14 days, one point per day.
    private static List<OccupancyPointDto> BuildDailyOccupancy(List<Attendance> all)
    {
        var since = DateTime.UtcNow.Date.AddDays(-13);
        return all
            .Where(a => a.Date >= since)
            .GroupBy(a => a.Date)
            .OrderBy(g => g.Key)
            .Select(g => new OccupancyPointDto { Label = g.Key.ToString("MMM dd"), VisitCount = g.Count() })
            .ToList();
    }

    // Grouped by day-of-week name, across all recorded history - answers
    // "which weekday is usually busiest?"
    private static List<OccupancyPointDto> BuildWeeklyOccupancy(List<Attendance> all)
    {
        var order = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        var grouped = all
            .GroupBy(a => a.Date.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Count());

        return order.Select(dayLabel =>
        {
            var dow = Array.IndexOf(order, dayLabel) == 6
                ? DayOfWeek.Sunday
                : (DayOfWeek)(Array.IndexOf(order, dayLabel) + 1);

            grouped.TryGetValue(dow, out var count);
            return new OccupancyPointDto { Label = dayLabel, VisitCount = count };
        }).ToList();
    }

    // Last 6 months, one point per month.
    private static List<OccupancyPointDto> BuildMonthlyOccupancy(List<Attendance> all)
    {
        var since = DateTime.UtcNow.Date.AddMonths(-5);
        var sinceMonthStart = new DateTime(since.Year, since.Month, 1);

        return all
            .Where(a => a.Date >= sinceMonthStart)
            .GroupBy(a => new DateTime(a.Date.Year, a.Date.Month, 1))
            .OrderBy(g => g.Key)
            .Select(g => new OccupancyPointDto { Label = g.Key.ToString("MMM yyyy"), VisitCount = g.Count() })
            .ToList();
    }

    // Buckets check-ins by hour-of-day (0-23) across all history, then derives
    // peak hours (busiest) and quiet hours (least busy, excluding hours with
    // zero recorded traffic, which just means "no data" rather than "quiet").
    private static List<HourlyLoadDto> BuildHourlyLoad(List<Attendance> all, out List<int> peakHours, out List<int> quietHours)
    {
        var hourly = Enumerable.Range(0, 24)
            .Select(h => new HourlyLoadDto { Hour = h, VisitCount = 0 })
            .ToList();

        foreach (var record in all)
        {
            var hour = record.CheckInTime.Hour;
            hourly[hour].VisitCount++;
        }

        peakHours = hourly
            .OrderByDescending(h => h.VisitCount)
            .Take(TopHoursCount)
            .Where(h => h.VisitCount > 0)
            .Select(h => h.Hour)
            .OrderBy(h => h)
            .ToList();

        quietHours = hourly
            .Where(h => h.VisitCount > 0) // ignore hours with no data at all
            .OrderBy(h => h.VisitCount)
            .Take(TopHoursCount)
            .Select(h => h.Hour)
            .OrderBy(h => h)
            .ToList();

        return hourly;
    }

    private static double BuildAverageDuration(List<Attendance> all)
    {
        var completed = all.Where(a => a.DurationMinutes.HasValue).ToList();
        if (completed.Count == 0) return 0;
        return Math.Round(completed.Average(a => a.DurationMinutes!.Value), 1);
    }

    private static string BuildBestTimeToVisitMessage(List<int> quietHours)
    {
        if (quietHours.Count == 0)
        {
            return "Not enough attendance history yet to recommend a best time to visit.";
        }

        var formatted = quietHours.Select(FormatHourRange);
        return $"Historically quieter around {string.Join(", ", formatted)}.";
    }

    private static string FormatHourRange(int hour)
    {
        var start = DateTime.Today.AddHours(hour).ToString("h tt");
        var end = DateTime.Today.AddHours(hour + 1).ToString("h tt");
        return $"{start}-{end}";
    }
}
