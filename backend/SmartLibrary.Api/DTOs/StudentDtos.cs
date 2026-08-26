namespace SmartLibrary.Api.DTOs;

// A row in the librarian's student directory — includes live status so the
// UI can show a "Check In" or "Check Out" button appropriately.
public class StudentDirectoryEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
    public bool IsCurrentlyInside { get; set; }
    public bool HasWebsiteAccount { get; set; }
}

// Used by the librarian to add a walk-in student who hasn't registered for
// website access yet, so they can be checked in immediately.
public class AddStudentRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string IndexNumber { get; set; } = string.Empty;
}
