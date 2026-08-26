using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public DashboardController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    // GET /api/dashboard — requires a website login, but either role
    // (Admin or Student) can view it. [Authorize] with no Roles means "any
    // authenticated user", which is exactly the "you must sign into the
    // website" rule from the spec.
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _libraryService.GetDashboardAsync();
        return Ok(new { success = true, data = result });
    }

    // PUT /api/dashboard/capacity — librarian only.
    [Authorize(Roles = "Admin")]
    [HttpPut("capacity")]
    public async Task<IActionResult> UpdateCapacity([FromBody] UpdateCapacityDto request)
    {
        await _libraryService.UpdateCapacityAsync(request.MaxCapacity);
        return Ok(new { success = true, message = "Library capacity updated." });
    }
}
