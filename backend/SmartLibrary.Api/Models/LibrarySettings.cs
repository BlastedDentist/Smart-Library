using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartLibrary.Api.Models;

// A singleton-style document (only ever one row in this collection) that
// stores configurable library settings, currently just maximum capacity.
public class LibrarySettings
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("maxCapacity")]
    public int MaxCapacity { get; set; } = 100;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
