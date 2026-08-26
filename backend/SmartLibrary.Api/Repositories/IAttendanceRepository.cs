using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public interface IAttendanceRepository
{
    Task<Attendance> CreateAsync(Attendance attendance);
    Task<Attendance?> GetActiveByIndexNumberAsync(string indexNumber);
    Task UpdateAsync(Attendance attendance);
    Task<int> CountCurrentlyInsideAsync();
    Task<List<Attendance>> GetByDateAsync(DateTime date);
    Task<List<Attendance>> GetAllAsync();
    Task<List<Attendance>> GetBetweenDatesAsync(DateTime start, DateTime end);
    Task<List<Attendance>> SearchAsync(string query);
}
