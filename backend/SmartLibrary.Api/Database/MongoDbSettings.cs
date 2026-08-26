namespace SmartLibrary.Api.Database;

// A plain C# class that mirrors the "MongoDbSettings" section of appsettings.json.
// ASP.NET Core's configuration binder maps the JSON keys onto these properties
// automatically (see Program.cs: builder.Services.Configure<MongoDbSettings>(...)).
// This pattern is called the "Options pattern" and keeps configuration strongly
// typed instead of scattering magic strings like Configuration["MongoDbSettings:ConnectionString"]
// throughout the codebase.
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string StudentsCollection { get; set; } = string.Empty;
    public string AttendanceCollection { get; set; } = string.Empty;
    public string LibrarySettingsCollection { get; set; } = string.Empty;
    public string BooksCollection { get; set; } = string.Empty;
    public string BookLoansCollection { get; set; } = string.Empty;
}
