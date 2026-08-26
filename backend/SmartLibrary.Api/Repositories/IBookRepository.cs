using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public interface IBookRepository
{
    Task<List<Book>> GetAllAsync();
    Task<List<Book>> SearchAsync(string query);
    Task<Book?> GetByIdAsync(string id);
    Task<Book?> GetByIsbnAsync(string isbn);
    Task<Book> CreateAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(string id);
}
