using MongoDB.Driver;
using SmartLibrary.Api.Database;
using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public class BookRepository : IBookRepository
{
    private readonly MongoDbContext _context;

    public BookRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Book>> GetAllAsync()
    {
        return await _context.Books.Find(_ => true).SortBy(b => b.Title).ToListAsync();
    }

    public async Task<List<Book>> SearchAsync(string query)
    {
        // Case-insensitive partial match on title, author, or category —
        // covers the natural ways a librarian would look for a book.
        var filter = Builders<Book>.Filter.Or(
            Builders<Book>.Filter.Regex(b => b.Title, new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<Book>.Filter.Regex(b => b.Author, new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<Book>.Filter.Regex(b => b.Category, new MongoDB.Bson.BsonRegularExpression(query, "i")),
            Builders<Book>.Filter.Regex(b => b.Isbn, new MongoDB.Bson.BsonRegularExpression(query, "i"))
        );

        return await _context.Books.Find(filter).SortBy(b => b.Title).ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(string id)
    {
        return await _context.Books.Find(b => b.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        return await _context.Books.Find(b => b.Isbn == isbn).FirstOrDefaultAsync();
    }

    public async Task<Book> CreateAsync(Book book)
    {
        await _context.Books.InsertOneAsync(book);
        return book;
    }

    public async Task UpdateAsync(Book book)
    {
        await _context.Books.ReplaceOneAsync(b => b.Id == book.Id, book);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Books.DeleteOneAsync(b => b.Id == id);
    }
}
