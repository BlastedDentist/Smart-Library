using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace SmartLibrary.Api.Services;

// Generates and validates short-lived QR tokens WITHOUT storing anything in
// a database or in server memory. Time is divided into fixed-size windows
// (e.g. every 30 seconds); the "token" is just an HMAC signature over the
// current window number, using a secret only the server knows. Anyone can
// see the token on the kiosk screen, but only the server can produce a
// signature that will validate — and it stops validating the moment the
// window rolls over, which is what makes a screenshotted/shared code
// useless a few seconds later.
//
// Because this is stateless (no "used tokens" list, no expiry cleanup job),
// it works correctly even if the API restarts or runs on multiple servers —
// there's nothing to keep in sync.
public class QrTokenService : IQrTokenService
{
    private readonly QrSettings _settings;

    public QrTokenService(IOptions<QrSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string Token, long ExpiresAtUnix, int WindowSeconds) GenerateCurrentToken()
    {
        var windowId = CurrentWindowId();
        var token = BuildToken(windowId);
        var expiresAtUnix = (windowId + 1) * _settings.WindowSeconds;
        return (token, expiresAtUnix, _settings.WindowSeconds);
    }

    public bool IsValid(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;

        var parts = token.Split('.');
        if (parts.Length != 2 || !long.TryParse(parts[0], out var windowId))
        {
            return false;
        }

        var currentWindowId = CurrentWindowId();

        // Accept the current window OR the immediately preceding one — a
        // one-window grace period absorbs network latency and the moment
        // between the kiosk screen rotating and the student's scan landing.
        var acceptableWindows = new[] { currentWindowId, currentWindowId - 1 };
        if (!acceptableWindows.Contains(windowId))
        {
            return false;
        }

        var expectedToken = BuildToken(windowId);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(token),
            Encoding.UTF8.GetBytes(expectedToken));
    }

    private long CurrentWindowId()
    {
        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return nowUnix / _settings.WindowSeconds;
    }

    private string BuildToken(long windowId)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.Secret));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(windowId.ToString()));
        var signatureHex = Convert.ToHexString(signatureBytes).ToLowerInvariant();
        return $"{windowId}.{signatureHex}";
    }
}
