using SmartLibrary.Api.DTOs;

namespace SmartLibrary.Api.Services;

public interface IBookService
{
    Task<List<BookResponseDto>> GetBooksAsync(string query);
    Task<BookResponseDto> GetBookAsync(string id);
    Task<BookResponseDto> AddBookAsync(CreateBookRequestDto request);
    Task<BookResponseDto> UpdateBookAsync(string id, UpdateBookRequestDto request);
    Task DeleteBookAsync(string id);
}
