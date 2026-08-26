using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

// Deliberately NOT a single class-level [Authorize] — the two actions here
// need two different roles (Admin generates the code, Student redeems it),
// and stacking [Authorize(Roles="Admin")] on the class with
// [Authorize(Roles="Student")] on a method would require BOTH roles at once
// (ASP.NET Core combines multiple [Authorize] attributes with AND, it
// doesn't let a method-level one override a class-level one) — so each
// action sets its own requirement instead.
[ApiController]
[Route("api/kiosk")]
public class KioskController : ControllerBase
{
    private readonly IQrTokenService _qrTokenService;
    private readonly IAttendanceService _attendanceService;

    public KioskController(IQrTokenService qrTokenService, IAttendanceService attendanceService)
    {
        _qrTokenService = qrTokenService;
        _attendanceService = attendanceService;
    }

    // GET /api/kiosk/token — librarian's kiosk display polls this to know
    // what QR code to show right now, and when to fetch the next one.
    [Authorize(Roles = "Admin")]
    [HttpGet("token")]
    public IActionResult GetToken()
    {
        var (token, expiresAtUnix, windowSeconds) = _qrTokenService.GenerateCurrentToken();
        var result = new KioskTokenDto { Token = token, ExpiresAtUnix = expiresAtUnix, WindowSeconds = windowSeconds };
        return Ok(new { success = true, data = result });
    }

    // POST /api/kiosk/scan — a student's phone calls this right after
    // scanning the kiosk's QR code. Their identity comes from their own
    // JWT (the "indexNumber" claim set at student login) — never from
    // anything in the request body, so there's no way to scan on someone
    // else's behalf just by knowing their index number.
    [Authorize(Roles = "Student")]
    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] ScanRequestDto request)
    {
        var indexNumber = User.FindFirstValue("indexNumber") ?? string.Empty;
        var result = await _attendanceService.ScanAsync(indexNumber, request.Token);
        return Ok(new { success = true, data = result });
    }
}
