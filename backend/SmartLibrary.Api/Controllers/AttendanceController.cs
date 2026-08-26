using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

// Checking students in/out of the PHYSICAL library is librarian-only —
// a student's own website login never grants this. That distinction is the
// whole point of the two account types.
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    // POST /api/attendance/check-in
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
    {
        var result = await _attendanceService.CheckInAsync(request);
        return Ok(new { success = true, data = result });
    }

    // POST /api/attendance/check-out
    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto request)
    {
        var result = await _attendanceService.CheckOutAsync(request);
        return Ok(new { success = true, data = result });
    }

    // GET /api/attendance/today
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var result = await _attendanceService.GetTodayAttendanceAsync();
        return Ok(new { success = true, data = result });
    }

    // GET /api/attendance/search?query=...
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query = "")
    {
        var result = await _attendanceService.SearchAttendanceAsync(query);
        return Ok(new { success = true, data = result });
    }

    // GET /api/attendance
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _attendanceService.GetAllAttendanceAsync();
        return Ok(new { success = true, data = result });
    }
}
