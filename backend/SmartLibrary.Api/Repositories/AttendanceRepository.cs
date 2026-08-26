using MongoDB.Driver;
using SmartLibrary.Api.Database;
using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly MongoDbContext _context;

    public AttendanceRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Attendance> CreateAsync(Attendance attendance)
    {
        await _context.Attendance.InsertOneAsync(attendance);
        return attendance;
    }

    public async Task<Attendance?> GetActiveByIndexNumberAsync(string indexNumber)
    {
        // "Active" means the student checked in but hasn't checked out yet.
        return await _context.Attendance
            .Find(a => a.IndexNumber == indexNumber && a.Status == "Inside")
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Attendance attendance)
    {
        await _context.Attendance.ReplaceOneAsync(a => a.Id == attendance.Id, attendance);
    }

    public async Task<int> CountCurrentlyInsideAsync()
    {
        return (int)await _context.Attendance.CountDocumentsAsync(a => a.Status == "Inside");
    }

    public async Task<List<Attendance>> GetByDateAsync(DateTime date)
    {
        var day = date.Date;
        var nextDay = day.AddDays(1);
        return await _context.Attendance
            .Find(a => a.Date >= day && a.Date < nextDay)
            .SortByDescending(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetAllAsync()
    {
        return await _context.Attendance
            .Find(_ => true)
            .SortByDescending(a => a.CheckInTime)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetBetweenDatesAsync(DateTime start, DateTime end)
    {
        return await _context.Attendance
            .Find(a => a.Date >= start.Date && a.Date <= end.Date)
            .ToListAsync();
    }

    public async Task<List<Attendance>> SearchAsync(string query)
    {
        var filter = Builders<Attendance>.Filter.Or(
            Builders<Attendance>.Filter.Regex(a => a.FullName, new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<Attendance>.Filter.Regex(a => a.IndexNumber, new MongoDB.Bson.BsonRegularExpression(query, "i"))
        );

        return await _context.Attendance
            .Find(filter)
            .SortByDescending(a => a.CheckInTime)
            .ToListAsync();
    }
}
