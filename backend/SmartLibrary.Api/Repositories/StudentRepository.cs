using MongoDB.Driver;
using SmartLibrary.Api.Database;
using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly MongoDbContext _context;

    public StudentRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIndexNumberAsync(string indexNumber)
    {
        return await _context.Students
            .Find(s => s.IndexNumber == indexNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<Student> CreateAsync(Student student)
    {
        await _context.Students.InsertOneAsync(student);
        return student;
    }

    public async Task UpdateAsync(Student student)
    {
        await _context.Students.ReplaceOneAsync(s => s.Id == student.Id, student);
    }

    public async Task<List<Student>> SearchAsync(string query)
    {
        // Case-insensitive partial match on name or index number.
        var filter = Builders<Student>.Filter.Or(
            Builders<Student>.Filter.Regex(s => s.FullName, new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<Student>.Filter.Regex(s => s.IndexNumber, new MongoDB.Bson.BsonRegularExpression(query, "i"))
        );

        return await _context.Students.Find(filter).ToListAsync();
    }

    public async Task<List<Student>> GetAllAsync()
    {
        return await _context.Students.Find(_ => true).SortBy(s => s.FullName).ToListAsync();
    }
}
