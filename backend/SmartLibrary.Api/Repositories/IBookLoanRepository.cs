using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public interface IBookLoanRepository
{
    Task<List<BookLoan>> GetAllAsync();
    Task<List<BookLoan>> GetByBookIdAsync(string bookId);
    Task<List<BookLoan>> GetByIndexNumberAsync(string indexNumber);
    Task<BookLoan?> GetByIdAsync(string id);
    Task<BookLoan?> GetActiveByBookAndStudentAsync(string bookId, string indexNumber);
    Task<BookLoan> CreateAsync(BookLoan loan);
    Task UpdateAsync(BookLoan loan);
}
