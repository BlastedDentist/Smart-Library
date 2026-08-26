namespace SmartLibrary.Api.DTOs;

// ---- Admin (librarian) login ----
public class AdminLoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// ---- Student account registration & login ----
// This is the student's WEBSITE account (view-only access to occupancy).
// It is separate from being checked in/out of the physical library, which
// only the librarian can do from the Admin panel.
public class StudentRegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class StudentLoginRequestDto
{
    public string IndexNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequestDto
{
    public string Identifier { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// ---- Shared login response, used by all three login/register flows ----
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Admin" | "Student"
    public string DisplayName { get; set; } = string.Empty; // admin username, or student full name
    public string? IndexNumber { get; set; } // only present for students
    public DateTime ExpiresAt { get; set; }
}
