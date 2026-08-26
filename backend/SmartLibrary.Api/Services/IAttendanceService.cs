using SmartLibrary.Api.DTOs;

namespace SmartLibrary.Api.Services;

public interface IAttendanceService
{
    Task<AttendanceResponseDto> CheckInAsync(CheckInRequestDto request);
    Task<AttendanceResponseDto> CheckOutAsync(CheckOutRequestDto request);
    Task<ScanResponseDto> ScanAsync(string indexNumber, string token);
    Task<List<AttendanceResponseDto>> GetTodayAttendanceAsync();
    Task<List<AttendanceResponseDto>> SearchAttendanceAsync(string query);
    Task<List<AttendanceResponseDto>> GetAllAttendanceAsync();
}
