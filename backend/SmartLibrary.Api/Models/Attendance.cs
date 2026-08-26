using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartLibrary.Api.Models;

// One document per library visit. A new Attendance record is created on
// check-in, and updated (CheckOutTime + DurationMinutes) on check-out.
public class Attendance
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("studentId")]
    public string? StudentId { get; set; }

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("indexNumber")]
    public string IndexNumber { get; set; } = string.Empty;

    [BsonElement("checkInTime")]
    public DateTime CheckInTime { get; set; }

    [BsonElement("checkOutTime")]
    public DateTime? CheckOutTime { get; set; }

    [BsonElement("durationMinutes")]
    public double? DurationMinutes { get; set; }

    // Stored as a UTC midnight date so we can efficiently group/query "today",
    // "this week", etc. without doing date-math on every read.
    [BsonElement("date")]
    public DateTime Date { get; set; }

    // "Inside" while the student hasn't checked out yet, "CheckedOut" after.
    [BsonElement("status")]
    public string Status { get; set; } = "Inside";
}
