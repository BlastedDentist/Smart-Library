using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Repositories;

namespace SmartLibrary.Api.Services;

public class LibraryService : ILibraryService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILibrarySettingsRepository _librarySettingsRepository;

    // These thresholds define the three occupancy states shown on the
    // dashboard. Keeping them as named constants (rather than inline magic
    // numbers) makes the business rule self-documenting and easy to tune.
    private const double AlmostFullThreshold = 0.85; // 85%+ = "Almost Full"

    public LibraryService(IAttendanceRepository attendanceRepository, ILibrarySettingsRepository librarySettingsRepository)
    {
        _attendanceRepository = attendanceRepository;
        _librarySettingsRepository = librarySettingsRepository;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var settings = await _librarySettingsRepository.GetSettingsAsync();
        var currentOccupancy = await _attendanceRepository.CountCurrentlyInsideAsync();

        var maxCapacity = settings.MaxCapacity <= 0 ? 1 : settings.MaxCapacity; // avoid divide-by-zero
        var percentage = Math.Round((double)currentOccupancy / maxCapacity * 100, 1);
        var availableSeats = Math.Max(settings.MaxCapacity - currentOccupancy, 0);

        string status;
        if (currentOccupancy >= settings.MaxCapacity)
        {
            status = "Library Full";
        }
        else if (percentage / 100.0 >= AlmostFullThreshold)
        {
            status = "Almost Full";
        }
        else
        {
            status = "Space Available";
        }

        return new DashboardDto
        {
            CurrentOccupancy = currentOccupancy,
            MaxCapacity = settings.MaxCapacity,
            AvailableSeats = availableSeats,
            OccupancyPercentage = percentage,
            LibraryStatus = status
        };
    }

    public async Task UpdateCapacityAsync(int maxCapacity)
    {
        if (maxCapacity <= 0)
        {
            throw new ArgumentException("Maximum capacity must be greater than zero.");
        }

        await _librarySettingsRepository.UpdateCapacityAsync(maxCapacity);
    }
}
