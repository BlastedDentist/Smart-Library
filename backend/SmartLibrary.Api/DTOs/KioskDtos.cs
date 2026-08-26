namespace SmartLibrary.Api.DTOs;

// Returned to the librarian's kiosk display so it knows what to render and
// when to fetch the next one.
public class KioskTokenDto
{
    public string Token { get; set; } = string.Empty;
    public long ExpiresAtUnix { get; set; }
    public int WindowSeconds { get; set; }
}

// Submitted by a student's phone after scanning the kiosk's QR code.
public class ScanRequestDto
{
    public string Token { get; set; } = string.Empty;
}

// "CheckedIn" or "CheckedOut" — lets the frontend show the right message
// without the student having to specify which one they meant (same tap-in/
// tap-out-on-the-same-reader model as a transit card).
public class ScanResponseDto
{
    public string Action { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double? DurationMinutes { get; set; } // only set when Action == "CheckedOut"
}
