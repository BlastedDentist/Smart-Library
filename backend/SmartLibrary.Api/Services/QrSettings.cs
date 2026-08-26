namespace SmartLibrary.Api.Services;

// Mirrors the "QrSettings" section of appsettings.json.
public class QrSettings
{
    public string Secret { get; set; } = string.Empty;
    public int WindowSeconds { get; set; } = 30; // how often the kiosk's QR code rotates
}
