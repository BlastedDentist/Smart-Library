using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartLibrary.Api.DTOs;
using SmartLibrary.Api.Models;
using SmartLibrary.Api.Repositories;

namespace SmartLibrary.Api.Services;

// Handles two entirely separate account systems that happen to share one
// token format:
//   - Admin (librarian): one configured account (username/password in
//     appsettings.json) — this is website login, and also grants permission
//     to check students in/out of the physical library.
//   - Student: self-service registered accounts, stored (with a hashed
//     password) on their Students-collection record — website login only.
//     Being logged in as a student never grants permission to check anyone
//     in or out; that stays librarian-only.
public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;
    private readonly AdminCredentials _adminCredentials;
    private readonly IStudentRepository _studentRepository;

    public AuthService(
        IOptions<JwtSettings> jwtSettings,
        IOptions<AdminCredentials> adminCredentials,
        IStudentRepository studentRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _adminCredentials = adminCredentials.Value;
        _studentRepository = studentRepository;
    }

    public LoginResponseDto? AdminLogin(AdminLoginRequestDto request)
    {
        var validUsername = string.Equals(request.Username, _adminCredentials.Username, StringComparison.Ordinal);
        var validPassword = string.Equals(request.Password, _adminCredentials.Password, StringComparison.Ordinal);

        if (!validUsername || !validPassword)
        {
            return null;
        }

        return BuildLoginResponse(role: "Admin", displayName: request.Username, indexNumber: null);
    }

    public async Task<LoginResponseDto> StudentRegisterAsync(StudentRegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.IndexNumber) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Full name, index number, and password are all required.");
        }

        if (request.Password.Length < 6)
        {
            throw new ArgumentException("Password must be at least 6 characters.");
        }

        var indexNumber = request.IndexNumber.Trim();
        var existing = await _studentRepository.GetByIndexNumberAsync(indexNumber);
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        if (existing == null)
        {
            // Brand new student, registering directly (no prior walk-in record).
            var student = new Student
            {
                FullName = request.FullName.Trim(),
                IndexNumber = indexNumber,
                PasswordHash = passwordHash
            };
            await _studentRepository.CreateAsync(student);
            return BuildLoginResponse(role: "Student", displayName: student.FullName, indexNumber: student.IndexNumber);
        }

        if (existing.PasswordHash != null)
        {
            // Someone already registered this index number — don't allow a
            // second account to silently overwrite their password.
            throw new InvalidOperationException("An account already exists for this index number. Please log in instead.");
        }

        // The librarian added this student as a walk-in previously (no
        // password yet) — attach the new password to that same record
        // instead of creating a duplicate, which the unique index on
        // indexNumber would reject anyway.
        existing.PasswordHash = passwordHash;
        existing.FullName = request.FullName.Trim();
        await _studentRepository.UpdateAsync(existing);

        return BuildLoginResponse(role: "Student", displayName: existing.FullName, indexNumber: existing.IndexNumber);
    }

    public async Task<LoginResponseDto?> StudentLoginAsync(StudentLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.IndexNumber) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var student = await _studentRepository.GetByIndexNumberAsync(request.IndexNumber.Trim());
        if (student == null || student.PasswordHash == null)
        {
            return null; // Deliberately the same "invalid" outcome as a wrong password — don't reveal which part was wrong.
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash);
        if (!passwordValid)
        {
            return null;
        }

        return BuildLoginResponse(role: "Student", displayName: student.FullName, indexNumber: student.IndexNumber);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var identifier = request.Identifier.Trim();

        if (string.Equals(identifier, _adminCredentials.Username, StringComparison.Ordinal))
        {
            var adminResult = AdminLogin(new AdminLoginRequestDto
            {
                Username = identifier,
                Password = request.Password
            });

            if (adminResult != null)
            {
                return adminResult;
            }
        }

        return await StudentLoginAsync(new StudentLoginRequestDto
        {
            IndexNumber = identifier,
            Password = request.Password
        });
    }

    private LoginResponseDto BuildLoginResponse(string role, string displayName, string? indexNumber)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        var token = GenerateToken(role, displayName, indexNumber, expiresAt);

        return new LoginResponseDto
        {
            Token = token,
            Role = role,
            DisplayName = displayName,
            IndexNumber = indexNumber,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateToken(string role, string displayName, string? indexNumber, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, displayName),
            new Claim(ClaimTypes.Role, role)
        };

        if (indexNumber != null)
        {
            claims.Add(new Claim("indexNumber", indexNumber));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
