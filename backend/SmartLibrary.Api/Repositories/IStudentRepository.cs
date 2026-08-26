using SmartLibrary.Api.Models;

namespace SmartLibrary.Api.Repositories;

public interface IStudentRepository
{
    Task<Student?> GetByIndexNumberAsync(string indexNumber);
    Task<Student> CreateAsync(Student student);
    Task UpdateAsync(Student student);
    Task<List<Student>> SearchAsync(string query);
    Task<List<Student>> GetAllAsync();
}
