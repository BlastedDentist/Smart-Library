using SmartLibrary.Api.DTOs;

namespace SmartLibrary.Api.Services;

public interface IStudentService
{
    Task<List<StudentDirectoryEntryDto>> GetDirectoryAsync(string query);
    Task<StudentDirectoryEntryDto> AddStudentAsync(AddStudentRequestDto request);
}
