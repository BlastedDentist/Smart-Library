using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Models;
using SmartLibrary.Api.Repositories;

namespace SmartLibrary.Api.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public StudentService(IStudentRepository studentRepository, IAttendanceRepository attendanceRepository)
    {
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<List<StudentDirectoryEntryDto>> GetDirectoryAsync(string query)
    {
        var students = string.IsNullOrWhiteSpace(query)
            ? await _studentRepository.GetAllAsync()
            : await _studentRepository.SearchAsync(query);

        var results = new List<StudentDirectoryEntryDto>();
        foreach (var student in students)
        {
            var active = await _attendanceRepository.GetActiveByIndexNumberAsync(student.IndexNumber);
            results.Add(new StudentDirectoryEntryDto
            {
                Id = student.Id ?? string.Empty,
                FullName = student.FullName,
                IndexNumber = student.IndexNumber,
                IsCurrentlyInside = active != null,
                HasWebsiteAccount = student.PasswordHash != null
            });
        }

        return results;
    }

    public async Task<StudentDirectoryEntryDto> AddStudentAsync(AddStudentRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.IndexNumber))
        {
            throw new ArgumentException("Full name and index number are required.");
        }

        var indexNumber = request.IndexNumber.Trim();
        var existing = await _studentRepository.GetByIndexNumberAsync(indexNumber);
        if (existing != null)
        {
            throw new InvalidOperationException("A student with this index number already exists in the directory.");
        }

        var student = new Student
        {
            FullName = request.FullName.Trim(),
            IndexNumber = indexNumber
            // PasswordHash stays null — this student hasn't registered for
            // website access, but the librarian can check them in/out right away.
        };

        var created = await _studentRepository.CreateAsync(student);

        return new StudentDirectoryEntryDto
        {
            Id = created.Id ?? string.Empty,
            FullName = created.FullName,
            IndexNumber = created.IndexNumber,
            IsCurrentlyInside = false,
            HasWebsiteAccount = false
        };
    }
}
