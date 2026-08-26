namespace SmartLibrary.Api.Services;

// Mirrors the "JwtSettings" section of appsettings.json (Options pattern,
// same idea as MongoDbSettings).
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 120;
}

// Mirrors the "AdminCredentials" section. For a real production system this
// would be a hashed password in the database - for this student project we
// keep a single configured admin account, which is simpler to reason about
// while still demonstrating a real login + JWT flow end-to-end.
public class AdminCredentials
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
