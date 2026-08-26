using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Models;
using SmartLibrary.Api.Repositories;

namespace SmartLibrary.Api.Services;

// This is where the borrow/return rules live — controllers stay thin,
// repositories stay dumb (see the comment atop AttendanceService for the
// same convention).
public class BookLoanService : IBookLoanService
{
    private readonly IBookLoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IStudentRepository _studentRepository;

    // Standard loan period. Matches the 14-day "recently added" window
    // BookService already uses elsewhere, for consistency.
    private static readonly TimeSpan LoanPeriod = TimeSpan.FromDays(14);

    public BookLoanService(
        IBookLoanRepository loanRepository,
        IBookRepository bookRepository,
        IStudentRepository studentRepository)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _studentRepository = studentRepository;
    }

    public async Task<BookLoanResponseDto> BorrowAsync(BorrowBookRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.BookId) || string.IsNullOrWhiteSpace(request.IndexNumber))
        {
            throw new ArgumentException("A book and a student are both required to authorize a borrow.");
        }

        var indexNumber = request.IndexNumber.Trim();

        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book == null)
        {
            throw new KeyNotFoundException("Book not found.");
        }

        // Same rule as check-in: the librarian picks the student from the
        // existing directory, so they MUST already be in it.
        var student = await _studentRepository.GetByIndexNumberAsync(indexNumber);
        if (student == null)
        {
            throw new InvalidOperationException("This student isn't in the directory yet. Add them first.");
        }

        if (book.AvailableCopies <= 0)
        {
            throw new InvalidOperationException($"No copies of \"{book.Title}\" are available to borrow right now.");
        }

        // Business rule: a student can't be authorized for a second copy of
        // the same title while they already have one out.
        var existingActive = await _loanRepository.GetActiveByBookAndStudentAsync(book.Id!, indexNumber);
        if (existingActive != null)
        {
            throw new InvalidOperationException($"{student.FullName} already has \"{book.Title}\" borrowed and hasn't returned it yet.");
        }

        var now = DateTime.UtcNow;
        var loan = new BookLoan
        {
            BookId = book.Id!,
            BookTitle = book.Title,
            Isbn = book.Isbn,
            StudentId = student.Id,
            StudentFullName = student.FullName,
            IndexNumber = indexNumber,
            BorrowedAt = now,
            DueAt = now.Add(LoanPeriod),
            Status = "Borrowed"
        };

        var created = await _loanRepository.CreateAsync(loan);

        book.AvailableCopies -= 1;
        await _bookRepository.UpdateAsync(book);

        return MapToDto(created);
    }

    public async Task<BookLoanResponseDto> ReturnAsync(ReturnBookRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.LoanId))
        {
            throw new ArgumentException("A loan must be specified to record a return.");
        }

        var loan = await _loanRepository.GetByIdAsync(request.LoanId);
        if (loan == null)
        {
            throw new KeyNotFoundException("Loan record not found.");
        }

        if (loan.Status == "Returned")
        {
            throw new InvalidOperationException($"\"{loan.BookTitle}\" was already marked returned.");
        }

        var now = DateTime.UtcNow;
        loan.ReturnedAt = now;
        loan.Status = "Returned";
        await _loanRepository.UpdateAsync(loan);

        // Give the copy back to the shelf — capped at TotalCopies so the
        // count can never drift upward if data is ever out of sync.
        var book = await _bookRepository.GetByIdAsync(loan.BookId);
        if (book != null)
        {
            book.AvailableCopies = Math.Min(book.AvailableCopies + 1, book.TotalCopies);
            await _bookRepository.UpdateAsync(book);
        }

        return MapToDto(loan);
    }

    public async Task<List<BookLoanResponseDto>> GetLoansForBookAsync(string bookId)
    {
        var loans = await _loanRepository.GetByBookIdAsync(bookId);
        return loans.Select(MapToDto).ToList();
    }

    public async Task<List<BookLoanResponseDto>> GetLoansForStudentAsync(string indexNumber)
    {
        var loans = await _loanRepository.GetByIndexNumberAsync(indexNumber);
        return loans.Select(MapToDto).ToList();
    }

    public async Task<List<BookLoanResponseDto>> GetAllLoansAsync()
    {
        var loans = await _loanRepository.GetAllAsync();
        return loans.Select(MapToDto).ToList();
    }

    private static BookLoanResponseDto MapToDto(BookLoan l)
    {
        var status = l.Status;
        if (status == "Borrowed" && DateTime.UtcNow > l.DueAt)
        {
            status = "Overdue";
        }

        return new BookLoanResponseDto
        {
            Id = l.Id ?? string.Empty,
            BookId = l.BookId,
            BookTitle = l.BookTitle,
            Isbn = l.Isbn,
            StudentFullName = l.StudentFullName,
            IndexNumber = l.IndexNumber,
            BorrowedAt = l.BorrowedAt,
            DueAt = l.DueAt,
            ReturnedAt = l.ReturnedAt,
            Status = status
        };
    }
}
