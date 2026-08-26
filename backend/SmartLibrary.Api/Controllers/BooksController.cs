using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

// Book catalog management is librarian-only, same as the student directory
// and attendance controls — there's no public "browse books" endpoint here.
// See the conversation notes in docs/API.md for a suggested future
// extension (a public catalog students can search/reserve from).
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET /api/books?query=...
    [HttpGet]
    public async Task<IActionResult> GetBooks([FromQuery] string query = "")
    {
        var result = await _bookService.GetBooksAsync(query);
        return Ok(new { success = true, data = result });
    }

    // GET /api/books/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(string id)
    {
        var result = await _bookService.GetBookAsync(id);
        return Ok(new { success = true, data = result });
    }

    // POST /api/books
    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] CreateBookRequestDto request)
    {
        var result = await _bookService.AddBookAsync(request);
        return Ok(new { success = true, data = result });
    }

    // PUT /api/books/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(string id, [FromBody] UpdateBookRequestDto request)
    {
        var result = await _bookService.UpdateBookAsync(id, request);
        return Ok(new { success = true, data = result });
    }

    // DELETE /api/books/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(string id)
    {
        await _bookService.DeleteBookAsync(id);
        return Ok(new { success = true, message = "Book removed from the catalog." });
    }
}
