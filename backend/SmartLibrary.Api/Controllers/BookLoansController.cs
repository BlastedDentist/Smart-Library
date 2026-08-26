using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

// Authorizing a borrow and logging a return are librarian-only actions, same
// as the rest of the catalog/attendance controls — a student's own login
// never lets them self-checkout a book.
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/loans")]
public class BookLoansController : ControllerBase
{
    private readonly IBookLoanService _loanService;

    public BookLoansController(IBookLoanService loanService)
    {
        _loanService = loanService;
    }

    // POST /api/loans/borrow
    [HttpPost("borrow")]
    public async Task<IActionResult> Borrow([FromBody] BorrowBookRequestDto request)
    {
        var result = await _loanService.BorrowAsync(request);
        return Ok(new { success = true, data = result });
    }

    // POST /api/loans/return
    [HttpPost("return")]
    public async Task<IActionResult> Return([FromBody] ReturnBookRequestDto request)
    {
        var result = await _loanService.ReturnAsync(request);
        return Ok(new { success = true, data = result });
    }

    // GET /api/loans/book/{bookId} — full borrow/return history for one title
    [HttpGet("book/{bookId}")]
    public async Task<IActionResult> GetForBook(string bookId)
    {
        var result = await _loanService.GetLoansForBookAsync(bookId);
        return Ok(new { success = true, data = result });
    }

    // GET /api/loans/student/{indexNumber} — full borrow/return history for one student
    [HttpGet("student/{indexNumber}")]
    public async Task<IActionResult> GetForStudent(string indexNumber)
    {
        var result = await _loanService.GetLoansForStudentAsync(indexNumber);
        return Ok(new { success = true, data = result });
    }

    // GET /api/loans — every loan across the whole library, newest first
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _loanService.GetAllLoansAsync();
        return Ok(new { success = true, data = result });
    }
}
