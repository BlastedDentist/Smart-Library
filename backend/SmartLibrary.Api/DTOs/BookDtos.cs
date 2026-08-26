namespace SmartLibrary.Api.DTOs;

public class BookResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }

    // True for the first couple of weeks after a book is added — lets the
    // frontend show a "New" badge without recomputing the cutoff itself.
    public bool IsRecentlyAdded { get; set; }
}

public class CreateBookRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class UpdateBookRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public string Description { get; set; } = string.Empty;
}
