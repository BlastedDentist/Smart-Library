using MongoDB.Driver;
using SmartLibrary.Api.Database;
using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public class LibrarySettingsRepository : ILibrarySettingsRepository
{
    private readonly MongoDbContext _context;

    public LibrarySettingsRepository(MongoDbContext context)
    {
        _context = context;
    }

    // The LibrarySettings collection is designed to hold exactly one document.
    // If it doesn't exist yet (first run), we create it with a sensible default.
    public async Task<LibrarySettings> GetSettingsAsync()
    {
        var settings = await _context.LibrarySettings.Find(_ => true).FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new LibrarySettings { MaxCapacity = 100 };
            await _context.LibrarySettings.InsertOneAsync(settings);
        }

        return settings;
    }

    public async Task UpdateCapacityAsync(int maxCapacity)
    {
        var settings = await GetSettingsAsync();
        var update = Builders<LibrarySettings>.Update
            .Set(s => s.MaxCapacity, maxCapacity)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);

        await _context.LibrarySettings.UpdateOneAsync(s => s.Id == settings.Id, update);
    }
}
