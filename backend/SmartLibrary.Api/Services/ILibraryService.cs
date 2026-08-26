using SmartLibrary.Api.DTOs;

namespace SmartLibrary.Api.Services;

public interface ILibraryService
{
    Task<DashboardDto> GetDashboardAsync();
    Task UpdateCapacityAsync(int maxCapacity);
}
