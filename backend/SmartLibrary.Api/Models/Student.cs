using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartLibrary.Api.Models;

// Represents a student record stored in the "Students" collection.
// This collection now serves two purposes: (1) it's the directory the
// librarian searches to check students in/out, and (2) it holds the
// credentials a student uses to log into the website themselves.
//
// A student can exist in this collection two ways:
//   - The librarian adds them as a walk-in (FullName + IndexNumber only,
//     PasswordHash stays null) so they can be checked in/out immediately.
//   - The student registers on the website themselves (sets a password).
// If a librarian-added student later registers, we attach the password to
// their existing record rather than creating a duplicate — see
// AuthService.StudentRegisterAsync.
public class Student
{
    // MongoDB requires an Id property. [BsonId] marks this as the primary key,
    // and [BsonRepresentation(BsonType.ObjectId)] lets us keep it as a C# string
    // in our code while MongoDB stores it internally as an ObjectId.
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    // Index numbers are unique per student (enforced via a unique index in Mongo,
    // see database/README.md for the setup script).
    [BsonElement("indexNumber")]
    public string IndexNumber { get; set; } = string.Empty;

    // Null until the student registers for website access. We never store
    // plain-text passwords — this holds a BCrypt hash.
    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
