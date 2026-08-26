using SmartLibrary.Api.DTOs;

namespace SmartLibrary.Api.Services;

public interface IAuthService
{
    LoginResponseDto? AdminLogin(AdminLoginRequestDto request);
    Task<LoginResponseDto> StudentRegisterAsync(StudentRegisterRequestDto request);
    Task<LoginResponseDto?> StudentLoginAsync(StudentLoginRequestDto request);
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
}
