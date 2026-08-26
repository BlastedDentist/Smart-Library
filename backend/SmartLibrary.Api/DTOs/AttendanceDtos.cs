namespace SmartLibrary.Api.DTOs;

// DTOs (Data Transfer Objects) are separate from our Models on purpose:
// Models map to what's stored in MongoDB; DTOs map to what the API accepts
// or returns over HTTP. Keeping them separate means we can change our
// database schema without breaking the frontend contract, and we never
// accidentally expose internal fields.

// The librarian selects the student from the directory in the Admin panel,
// so check-in only needs the index number to look up who it is — the name
// is already known from the Students collection.
public class CheckInRequestDto
{
    public string IndexNumber { get; set; } = string.Empty;
}

public class CheckOutRequestDto
{
    public string IndexNumber { get; set; } = string.Empty;
}

public class AttendanceResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? DurationMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
}
