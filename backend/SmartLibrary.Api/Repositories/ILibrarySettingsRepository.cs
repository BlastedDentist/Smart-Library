using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public interface ILibrarySettingsRepository
{
    Task<LibrarySettings> GetSettingsAsync();
    Task UpdateCapacityAsync(int maxCapacity);
}
