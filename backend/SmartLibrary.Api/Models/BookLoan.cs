using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartLibrary.Api.Models;

// One document per borrow, stored in the "BookLoans" collection — mirrors
// the Attendance pattern (a record is created on check-in/borrow, then the
// SAME record is updated on check-out/return) rather than deleting anything,
// so there's always a full history of who borrowed what and when.
//
// Borrowing is librarian-authorized, not self-service: a student can't grant
// themselves a loan by being logged in, the same way a student login never
// grants attendance check-in/out rights. The librarian picks the book and
// the student from the directory, same UX as attendance.
public class BookLoan
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("bookId")]
    public string BookId { get; set; } = string.Empty;

    // Denormalized so the loan history still reads clearly even if the book
    // is later edited (title change) or removed from the catalog.
    [BsonElement("bookTitle")]
    public string BookTitle { get; set; } = string.Empty;

    [BsonElement("isbn")]
    public string Isbn { get; set; } = string.Empty;

    [BsonElement("studentId")]
    public string? StudentId { get; set; }

    [BsonElement("studentFullName")]
    public string StudentFullName { get; set; } = string.Empty;

    [BsonElement("indexNumber")]
    public string IndexNumber { get; set; } = string.Empty;

    [BsonElement("borrowedAt")]
    public DateTime BorrowedAt { get; set; }

    // Two weeks is the same "grace window" convention BookService already
    // uses for "recently added" — reused here as the standard loan period.
    [BsonElement("dueAt")]
    public DateTime DueAt { get; set; }

    [BsonElement("returnedAt")]
    public DateTime? ReturnedAt { get; set; }

    // "Borrowed" while out, "Returned" once the librarian logs its return.
    [BsonElement("status")]
    public string Status { get; set; } = "Borrowed";
}
