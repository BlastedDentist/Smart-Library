# API Reference

Base URL (dev): `http://localhost:5000/api`
Interactive docs: `http://localhost:5000/swagger`

All responses are wrapped as `{ "success": bool, "data": ..., "message": ... }`.

## Two separate account types

| | Website login | Can check students in/out of the physical library? |
|---|---|---|
| **Admin (librarian)** | `POST /auth/admin/login` | Yes |
| **Student** | `POST /auth/student/register` then `/auth/student/login` | No — view-only |

Every protected endpoint below requires `Authorization: Bearer <token>` from
one of the three login/register calls. 🔒 = any logged-in user (either
role). 🔒Admin = librarian only.

## Auth

### `POST /auth/admin/login`
**Body:** `{ "username": "admin", "password": "Admin@123" }`

### `POST /auth/student/register`
Creates a student's website account. If the librarian already added this
student as a walk-in (no password yet), this attaches a password to that
existing directory record instead of creating a duplicate.

**Body:** `{ "fullName": "Ama Boateng", "indexNumber": "10912345", "password": "your-password" }`

### `POST /auth/student/login`
**Body:** `{ "indexNumber": "10912345", "password": "your-password" }`

All three return the same shape:
```json
{
  "success": true,
  "data": {
    "token": "...",
    "role": "Admin | Student",
    "displayName": "admin or Ama Boateng",
    "indexNumber": "10912345 (null for Admin)",
    "expiresAt": "..."
  }
}
```

## Attendance — 🔒Admin — physical library sign-in/out

These endpoints actually change who's inside the building. A student's own
login can never call these — only the librarian.

### `POST /attendance/check-in`
**Body:** `{ "indexNumber": "10912345" }`
The student must already exist in the directory (self-registered, or added
as a walk-in). Fails with 400 if they're already signed in.

### `POST /attendance/check-out`
**Body:** `{ "indexNumber": "10912345" }`
Fails with 400 if there's no active check-in for that index number.

### `GET /attendance/today`
Every attendance record for today (inside and checked-out).

### `GET /attendance/search?query=`
Case-insensitive partial match against name or index number.

### `GET /attendance`
Full attendance history.

## Student directory — 🔒Admin

### `GET /student/directory?query=`
Every known student, with live status: `isCurrentlyInside` and
`hasWebsiteAccount`. This is what the librarian's Check In/Out buttons are
built from.

### `POST /student`
Adds a walk-in student who hasn't registered online yet, so they can be
checked in immediately.
**Body:** `{ "fullName": "Kwame Mensah", "indexNumber": "10998765" }`

## Dashboard

### `GET /dashboard` — 🔒 any logged-in user
```json
{
  "currentOccupancy": 42,
  "maxCapacity": 100,
  "availableSeats": 58,
  "occupancyPercentage": 42.0,
  "libraryStatus": "Space Available"
}
```
`libraryStatus` is one of `"Space Available"`, `"Almost Full"` (>=85%), `"Library Full"` (at/over capacity).

### `PUT /dashboard/capacity` — 🔒Admin
**Body:** `{ "maxCapacity": 120 }`

## Analytics

### `GET /analytics/summary` — 🔒 any logged-in user
Daily (last 14 days), weekly (by day-of-week), and monthly (last 6 months)
visit counts, hourly load (0-23), computed peak/quiet hours, average visit
duration, and a plain-language "best time to visit" recommendation.

## Book catalog — 🔒Admin

### `GET /books?query=`
Case-insensitive partial match against title, author, category, or ISBN.
Leave `query` blank to get the full catalog.

### `GET /books/{id}`

### `POST /books`
**Body:** `{ "title", "author", "isbn", "category", "totalCopies", "description" }`
Rejects a duplicate ISBN with 400 — edit the existing entry instead.
`availableCopies` starts equal to `totalCopies`.

### `PUT /books/{id}`
Same body shape as create, plus `availableCopies` — this is how a librarian
marks copies as checked out or returned.

### `DELETE /books/{id}`

```json
{
  "id": "...",
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "category": "Computer Science",
  "totalCopies": 5,
  "availableCopies": 3,
  "description": "...",
  "addedAt": "...",
  "isRecentlyAdded": true
}
```
`isRecentlyAdded` is `true` for two weeks after a book is added — powers the
"New" badge in the catalog view.

## Kiosk / QR check-in

### `GET /kiosk/token` — 🔒Admin
Returns the current, valid QR code value for the kiosk screen to render,
plus when it expires so the frontend knows when to fetch the next one.
```json
{ "token": "58234219.9f2a1c...", "expiresAtUnix": 1752812460, "windowSeconds": 30 }
```
The token is a stateless HMAC signature over a 30-second time window (see
`QrTokenService.cs`) — nothing is stored in the database or in server
memory, so it works correctly even across restarts or multiple server
instances.

### `POST /kiosk/scan` — 🔒 Student only
**Body:** `{ "token": "58234219.9f2a1c..." }`

The student's identity comes entirely from their own JWT (the `indexNumber`
claim set at student login) — never from the request body — so there's no
way to scan on someone else's behalf just by knowing their index number.
Automatically decides check-in vs. check-out based on the student's current
status (same "tap the same reader either way" model as a transit card).

Fails with 400 if the token doesn't match the kiosk's current (or
immediately previous, for clock-skew tolerance) window.

```json
{
  "success": true,
  "data": { "action": "CheckedIn | CheckedOut", "fullName": "...", "timestamp": "...", "durationMinutes": null }
}
```
`durationMinutes` is only populated when `action` is `"CheckedOut"`.

## Error format

Non-2xx responses follow the same envelope with `success: false`:
```json
{ "success": false, "message": "This student is already signed in. Please sign out first." }
```

A 401 typically means a missing/expired/wrong-role token — check the
`Authorization` header and which role the endpoint needs.
