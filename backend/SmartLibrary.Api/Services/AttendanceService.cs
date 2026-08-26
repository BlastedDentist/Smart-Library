using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Models;
using SmartLibrary.Api.Repositories;

namespace SmartLibrary.Api.Services;

// This is where the actual business rules live - "you can't check in twice
// without checking out first", "duration is calculated on checkout", etc.
// Controllers stay thin (just translate HTTP <-> DTOs) and Repositories stay
// dumb (just talk to Mongo); all decision-making happens here.
public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IQrTokenService _qrTokenService;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        IQrTokenService qrTokenService)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _qrTokenService = qrTokenService;
    }

    public async Task<AttendanceResponseDto> CheckInAsync(CheckInRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.IndexNumber))
        {
            throw new ArgumentException("Index number is required.");
        }

        var indexNumber = request.IndexNumber.Trim();

        // The librarian selects students from the existing directory rather
        // than typing their name, so the student MUST already exist here —
        // either self-registered, or added as a walk-in via the Admin panel.
        var student = await _studentRepository.GetByIndexNumberAsync(indexNumber);
        if (student == null)
        {
            throw new InvalidOperationException("This student isn't in the directory yet. Add them first.");
        }

        // Business rule: a student who is already signed in cannot sign in again.
        var existingActive = await _attendanceRepository.GetActiveByIndexNumberAsync(indexNumber);
        if (existingActive != null)
        {
            throw new InvalidOperationException("This student is already signed in. Please sign out first.");
        }

        var now = DateTime.UtcNow;
        var attendance = new Attendance
        {
            StudentId = student.Id,
            FullName = student.FullName,
            IndexNumber = indexNumber,
            CheckInTime = now,
            Date = now.Date,
            Status = "Inside"
        };

        var created = await _attendanceRepository.CreateAsync(attendance);
        return MapToDto(created);
    }

    public async Task<AttendanceResponseDto> CheckOutAsync(CheckOutRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.IndexNumber))
        {
            throw new ArgumentException("Index number is required.");
        }

        var indexNumber = request.IndexNumber.Trim();
        var active = await _attendanceRepository.GetActiveByIndexNumberAsync(indexNumber);

        if (active == null)
        {
            throw new InvalidOperationException("No active check-in found for this index number.");
        }

        var now = DateTime.UtcNow;
        active.CheckOutTime = now;
        active.DurationMinutes = (now - active.CheckInTime).TotalMinutes;
        active.Status = "CheckedOut";

        await _attendanceRepository.UpdateAsync(active);
        return MapToDto(active);
    }

    // The self-service counterpart to CheckInAsync/CheckOutAsync. Whereas
    // those two are librarian-only and explicit about which action they
    // perform, a QR scan is "tap the same reader either way" — so this
    // decides in/out automatically based on the student's current status,
    // the same way a transit card works whether you're entering or leaving.
    //
    // The `token` is what makes this safe to expose to students at all: it
    // must be a currently-valid, freshly-generated code from the kiosk
    // screen (see QrTokenService) — proof the student's phone was physically
    // in front of that screen within the last ~30-60 seconds, not just proof
    // they know their own index number.
    public async Task<ScanResponseDto> ScanAsync(string indexNumber, string token)
    {
        if (!_qrTokenService.IsValid(token))
        {
            throw new ArgumentException("This QR code has expired. Please scan the current code on the kiosk screen.");
        }

        if (string.IsNullOrWhiteSpace(indexNumber))
        {
            throw new ArgumentException("Index number is required.");
        }

        var student = await _studentRepository.GetByIndexNumberAsync(indexNumber);
        if (student == null)
        {
            // Shouldn't normally happen — a logged-in student account always
            // has a matching directory record — but guards against edge cases.
            throw new InvalidOperationException("Student record not found.");
        }

        var active = await _attendanceRepository.GetActiveByIndexNumberAsync(indexNumber);
        var now = DateTime.UtcNow;

        if (active == null)
        {
            // Not currently inside -> this scan is a check-in.
            var attendance = new Attendance
            {
                StudentId = student.Id,
                FullName = student.FullName,
                IndexNumber = indexNumber,
                CheckInTime = now,
                Date = now.Date,
                Status = "Inside"
            };
            var created = await _attendanceRepository.CreateAsync(attendance);

            return new ScanResponseDto
            {
                Action = "CheckedIn",
                FullName = created.FullName,
                Timestamp = created.CheckInTime,
                DurationMinutes = null
            };
        }

        // Currently inside -> this scan is a check-out.
        active.CheckOutTime = now;
        active.DurationMinutes = (now - active.CheckInTime).TotalMinutes;
        active.Status = "CheckedOut";
        await _attendanceRepository.UpdateAsync(active);

        return new ScanResponseDto
        {
            Action = "CheckedOut",
            FullName = active.FullName,
            Timestamp = now,
            DurationMinutes = active.DurationMinutes
        };
    }

    public async Task<List<AttendanceResponseDto>> GetTodayAttendanceAsync()
    {
        var records = await _attendanceRepository.GetByDateAsync(DateTime.UtcNow.Date);
        return records.Select(MapToDto).ToList();
    }

    public async Task<List<AttendanceResponseDto>> SearchAttendanceAsync(string query)
    {
        var records = await _attendanceRepository.SearchAsync(query);
        return records.Select(MapToDto).ToList();
    }

    public async Task<List<AttendanceResponseDto>> GetAllAttendanceAsync()
    {
        var records = await _attendanceRepository.GetAllAsync();
        return records.Select(MapToDto).ToList();
    }

    private static AttendanceResponseDto MapToDto(Attendance a) => new()
    {
        Id = a.Id ?? string.Empty,
        FullName = a.FullName,
        IndexNumber = a.IndexNumber,
        CheckInTime = a.CheckInTime,
        CheckOutTime = a.CheckOutTime,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status
    };
}
