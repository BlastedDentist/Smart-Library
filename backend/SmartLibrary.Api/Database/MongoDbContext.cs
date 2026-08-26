using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Database;

// A thin wrapper around the MongoDB client that exposes strongly typed
// IMongoCollection<T> properties. Repositories depend on this class instead
// of talking to MongoDB.Driver directly, which keeps all connection/collection
// wiring in exactly one place.
//
// This is registered as a Singleton in Program.cs because MongoClient is
// thread-safe and expensive to create - the official driver guidance is to
// create one MongoClient per application and reuse it.
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> mongoDbSettings)
    {
        _settings = mongoDbSettings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<Student> Students =>
        _database.GetCollection<Student>(_settings.StudentsCollection);

    public IMongoCollection<Attendance> Attendance =>
        _database.GetCollection<Attendance>(_settings.AttendanceCollection);

    public IMongoCollection<LibrarySettings> LibrarySettings =>
        _database.GetCollection<LibrarySettings>(_settings.LibrarySettingsCollection);

    public IMongoCollection<Book> Books =>
        _database.GetCollection<Book>(_settings.BooksCollection);

    public IMongoCollection<BookLoan> BookLoans =>
        _database.GetCollection<BookLoan>(_settings.BookLoansCollection);
}
