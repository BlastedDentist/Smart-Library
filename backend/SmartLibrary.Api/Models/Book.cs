using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartLibrary.Api.Models;

// Represents a book title in the library's catalog, stored in the "Books"
// collection. This tracks copies at the TITLE level (e.g. "5 copies of
// Clean Code"), not one document per physical book — that's the right
// granularity for "how many are available right now" without needing to
// individually barcode every copy.
public class Book
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("author")]
    public string Author { get; set; } = string.Empty;

    // ISBNs are kept unique per title (enforced via a unique index in
    // Mongo — see database/README.md) so the same book can't accidentally
    // be catalogued twice under two different records.
    [BsonElement("isbn")]
    public string Isbn { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty; // e.g. "Fiction", "Computer Science"

    [BsonElement("totalCopies")]
    public int TotalCopies { get; set; }

    [BsonElement("availableCopies")]
    public int AvailableCopies { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("addedAt")]
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
