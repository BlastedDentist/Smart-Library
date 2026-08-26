namespace SmartLibrary.Api.DTOs;

// The librarian picks the book (already open in Book Management) and the
// student (from the directory), same shape as CheckInRequestDto.
public class BorrowBookRequestDto
{
    public string BookId { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
}

public class ReturnBookRequestDto
{
    public string LoanId { get; set; } = string.Empty;
}

public class BookLoanResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string BookId { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
    public DateTime BorrowedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }

    // "Borrowed" | "Overdue" | "Returned" — Overdue is derived (Borrowed +
    // past its due date) rather than stored, so it's always accurate without
    // a background job to flip statuses.
    public string Status { get; set; } = string.Empty;
}
