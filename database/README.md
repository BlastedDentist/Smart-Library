# Database Setup — MongoDB

The API creates collections automatically the first time it writes to them,
so you don't need to manually create `Students`, `Attendance`, or
`LibrarySettings`. You DO need to run the index script below once, so that
index numbers are guaranteed unique at the database level (never rely on
application code alone to enforce uniqueness).

## Option A — MongoDB Atlas (recommended)

1. Create a free cluster at https://www.mongodb.com/cloud/atlas
2. Create a database user (username/password).
3. Under Network Access, allow your current IP (or 0.0.0.0/0 for local dev only).
4. Copy the connection string and paste it into
   `backend/SmartLibrary.Api/appsettings.Development.json` as
   `MongoDbSettings:ConnectionString`.

## Option B — Local MongoDB

Install MongoDB Community Server, then it will run at `mongodb://localhost:27017`
by default — that's already what `appsettings.json` points to.

## Run this once (via `mongosh`, or MongoDB Compass's shell tab)

```js
use SmartLibraryDb

db.Students.createIndex({ indexNumber: 1 }, { unique: true })
db.Attendance.createIndex({ indexNumber: 1, status: 1 })
db.Attendance.createIndex({ date: 1 })
db.Books.createIndex({ isbn: 1 }, { unique: true })
db.BookLoans.createIndex({ bookId: 1, indexNumber: 1, status: 1 })
db.BookLoans.createIndex({ indexNumber: 1 })
```

- The unique index on `Students.indexNumber` stops the same index number
  ever being registered as two different students.
- The compound index on `Attendance.indexNumber + status` speeds up the
  "does this student already have an active check-in?" lookup that runs on
  every check-in.
- The index on `Attendance.date` speeds up the "today's attendance" and
  analytics queries.
- The compound index on `BookLoans.bookId + indexNumber + status` speeds up
  the "does this student already have this title out?" check that runs on
  every borrow; the `indexNumber` index speeds up looking up a single
  student's full borrowing history.

## Collections

| Collection | Purpose |
|---|---|
| `Students` | One document per known student (name + index number). Grows automatically as new students check in for the first time. |
| `Attendance` | One document per library visit (check-in → check-out). |
| `LibrarySettings` | Single document holding configurable settings (currently: `maxCapacity`). |
| `Books` | One document per book title in the catalog, with total/available copy counts. Managed entirely from the librarian's Book Management page. |
| `BookLoans` | One document per borrow (librarian-authorized), updated in place when the librarian logs the return. This is the "who has this book / who returned it" history. |
