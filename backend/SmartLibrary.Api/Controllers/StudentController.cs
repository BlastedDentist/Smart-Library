using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

// Everything here is librarian-only: this is the directory the Admin panel
// uses to find a student and check them in/out, or add a walk-in.
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    // GET /api/student/directory?query=...
    [HttpGet("directory")]
    public async Task<IActionResult> GetDirectory([FromQuery] string query = "")
    {
        var result = await _studentService.GetDirectoryAsync(query);
        return Ok(new { success = true, data = result });
    }

    // POST /api/student — add a walk-in student who hasn't registered online.
    [HttpPost]
    public async Task<IActionResult> AddStudent([FromBody] AddStudentRequestDto request)
    {
        var result = await _studentService.AddStudentAsync(request);
        return Ok(new { success = true, data = result });
    }
}
