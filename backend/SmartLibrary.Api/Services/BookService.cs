using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Models;
using SmartLibrary.Api.Repositories;

namespace SmartLibrary.Api.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    // A book counts as "New" in the catalog for two weeks after it's added —
    // just long enough for a librarian glancing at the list to notice a
    // recent addition without it lingering as "new" forever.
    private static readonly TimeSpan RecentWindow = TimeSpan.FromDays(14);

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<List<BookResponseDto>> GetBooksAsync(string query)
    {
        var books = string.IsNullOrWhiteSpace(query)
            ? await _bookRepository.GetAllAsync()
            : await _bookRepository.SearchAsync(query);

        return books.Select(MapToDto).ToList();
    }

    public async Task<BookResponseDto> GetBookAsync(string id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            throw new KeyNotFoundException("Book not found.");
        }
        return MapToDto(book);
    }

    public async Task<BookResponseDto> AddBookAsync(CreateBookRequestDto request)
    {
        ValidateInput(request.Title, request.Author, request.Isbn, request.TotalCopies);

        var isbn = request.Isbn.Trim();
        var existing = await _bookRepository.GetByIsbnAsync(isbn);
        if (existing != null)
        {
            throw new InvalidOperationException($"A book with ISBN {isbn} is already in the catalog (\"{existing.Title}\"). Edit that entry instead of adding a duplicate.");
        }

        var book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Isbn = isbn,
            Category = request.Category.Trim(),
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.TotalCopies, // every copy of a newly-catalogued book starts on the shelf
            Description = request.Description.Trim()
        };

        var created = await _bookRepository.CreateAsync(book);
        return MapToDto(created);
    }

    public async Task<BookResponseDto> UpdateBookAsync(string id, UpdateBookRequestDto request)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            throw new KeyNotFoundException("Book not found.");
        }

        ValidateInput(request.Title, request.Author, request.Isbn, request.TotalCopies);

        if (request.AvailableCopies < 0 || request.AvailableCopies > request.TotalCopies)
        {
            throw new ArgumentException("Available copies must be between 0 and the total number of copies.");
        }

        var isbn = request.Isbn.Trim();
        if (isbn != book.Isbn)
        {
            // ISBN changed — re-check uniqueness against every OTHER book.
            var existing = await _bookRepository.GetByIsbnAsync(isbn);
            if (existing != null && existing.Id != book.Id)
            {
                throw new InvalidOperationException($"Another book already uses ISBN {isbn} (\"{existing.Title}\").");
            }
        }

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Isbn = isbn;
        book.Category = request.Category.Trim();
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.AvailableCopies;
        book.Description = request.Description.Trim();

        await _bookRepository.UpdateAsync(book);
        return MapToDto(book);
    }

    public async Task DeleteBookAsync(string id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null)
        {
            throw new KeyNotFoundException("Book not found.");
        }
        await _bookRepository.DeleteAsync(id);
    }

    private static void ValidateInput(string title, string author, string isbn, int totalCopies)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("Title, author, and ISBN are all required.");
        }
        if (totalCopies <= 0)
        {
            throw new ArgumentException("Total copies must be at least 1.");
        }
    }

    private static BookResponseDto MapToDto(Book b) => new()
    {
        Id = b.Id ?? string.Empty,
        Title = b.Title,
        Author = b.Author,
        Isbn = b.Isbn,
        Category = b.Category,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies,
        Description = b.Description,
        AddedAt = b.AddedAt,
        IsRecentlyAdded = DateTime.UtcNow - b.AddedAt <= RecentWindow
    };
}
