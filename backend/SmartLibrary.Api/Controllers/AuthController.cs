
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/auth/admin/login — librarian website login.
    [HttpPost("admin/login")]
    public IActionResult AdminLogin([FromBody] AdminLoginRequestDto request)
    {
        var result = _authService.AdminLogin(request);
        if (result == null)
        {
            return Unauthorized(new { success = false, message = "Invalid username or password." });
        }
        return Ok(new { success = true, data = result });
    }

    // POST /api/auth/student/register — student creates their website account.
    [HttpPost("student/register")]
    public async Task<IActionResult> StudentRegister([FromBody] StudentRegisterRequestDto request)
    {
        var result = await _authService.StudentRegisterAsync(request);
        return Ok(new { success = true, data = result });
    }

    // POST /api/auth/login — shared login for both students and admins.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(new { success = false, message = "Invalid username/index number or password." });
        }
        return Ok(new { success = true, data = result });
    }

    // POST /api/auth/student/login — student website login.
    [HttpPost("student/login")]
    public async Task<IActionResult> StudentLogin([FromBody] StudentLoginRequestDto request)
    {
        var result = await _authService.StudentLoginAsync(request);
        if (result == null)
        {
            return Unauthorized(new { success = false, message = "Invalid index number or password." });
        }
        return Ok(new { success = true, data = result });
    }
}
