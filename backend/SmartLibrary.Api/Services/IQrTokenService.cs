namespace SmartLibrary.Api.Services;

public interface IQrTokenService
{
    (string Token, long ExpiresAtUnix, int WindowSeconds) GenerateCurrentToken();
    bool IsValid(string token);
}
