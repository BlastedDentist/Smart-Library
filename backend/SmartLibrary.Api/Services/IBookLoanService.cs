using SmartLibrary.Api.DTOs;

namespace SmartLibrary.Api.Services;

public interface IBookLoanService
{
    Task<BookLoanResponseDto> BorrowAsync(BorrowBookRequestDto request);
    Task<BookLoanResponseDto> ReturnAsync(ReturnBookRequestDto request);
    Task<List<BookLoanResponseDto>> GetLoansForBookAsync(string bookId);
    Task<List<BookLoanResponseDto>> GetLoansForStudentAsync(string indexNumber);
    Task<List<BookLoanResponseDto>> GetAllLoansAsync();
}
