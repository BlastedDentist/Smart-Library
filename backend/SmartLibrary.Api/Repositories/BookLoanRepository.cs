using MongoDB.Driver;
using SmartLibrary.Api.Database;
using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public class BookLoanRepository : IBookLoanRepository
{
    private readonly MongoDbContext _context;

    public BookLoanRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookLoan>> GetAllAsync()
    {
        return await _context.BookLoans.Find(_ => true).SortByDescending(l => l.BorrowedAt).ToListAsync();
    }

    public async Task<List<BookLoan>> GetByBookIdAsync(string bookId)
    {
        return await _context.BookLoans.Find(l => l.BookId == bookId).SortByDescending(l => l.BorrowedAt).ToListAsync();
    }

    public async Task<List<BookLoan>> GetByIndexNumberAsync(string indexNumber)
    {
        return await _context.BookLoans.Find(l => l.IndexNumber == indexNumber).SortByDescending(l => l.BorrowedAt).ToListAsync();
    }

    public async Task<BookLoan?> GetByIdAsync(string id)
    {
        return await _context.BookLoans.Find(l => l.Id == id).FirstOrDefaultAsync();
    }

    public async Task<BookLoan?> GetActiveByBookAndStudentAsync(string bookId, string indexNumber)
    {
        return await _context.BookLoans
            .Find(l => l.BookId == bookId && l.IndexNumber == indexNumber && l.Status == "Borrowed")
            .FirstOrDefaultAsync();
    }

    public async Task<BookLoan> CreateAsync(BookLoan loan)
    {
        await _context.BookLoans.InsertOneAsync(loan);
        return loan;
    }

    public async Task UpdateAsync(BookLoan loan)
    {
        await _context.BookLoans.ReplaceOneAsync(l => l.Id == loan.Id, loan);
    }
}
