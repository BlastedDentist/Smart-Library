# UMaT Smart Library Occupancy Monitoring and Analytics System

A full-stack web app that replaces guesswork ("is the library full?") with
real data. Students sign in/out with just their name and index number — no
RFID — and the system tracks live occupancy plus historical patterns like
peak and quiet hours.

```
Frontend (React)  →  ASP.NET Core Web API (C#)  →  MongoDB
```

## Tech stack

| Layer | Technology |
|---|---|
| Frontend | React (Vite), React Router, Axios, Chart.js |
| Backend | ASP.NET Core Web API (.NET 8, C#) |
| Database | MongoDB |
| Auth | JWT Bearer (admin-only endpoints) |
| API docs | Swagger / OpenAPI (dev mode) |

## Project structure

```
SmartLibrary/
├── backend/
│   └── SmartLibrary.Api/
│       ├── Controllers/     # HTTP endpoints — thin, delegate to Services
│       ├── Models/          # MongoDB document shapes
│       ├── DTOs/            # Request/response shapes for the API contract
│       ├── Services/        # Business logic
│       ├── Repositories/    # MongoDB data access
│       ├── Database/        # Mongo connection + settings
│       ├── Middleware/      # Global exception handling
│       ├── Program.cs       # App startup / DI wiring
│       └── appsettings.json
├── frontend/
│   └── src/
│       ├── components/      # Reusable UI pieces (SeatGrid, Navbar, StatCard…)
│       ├── pages/           # Route-level views
│       ├── layouts/         # Page shell (Navbar + content)
│       ├── services/        # api.js — all HTTP calls live here
│       ├── hooks/           # useOccupancy, useAdminAuth
│       └── styles/          # Design tokens + global CSS
├── database/
│   └── README.md            # MongoDB index setup script
└── docs/
    ├── API.md                # Endpoint reference
    └── SETUP.md               # Step-by-step first-run guide
```

## Quick start

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download), [Node.js 18+](https://nodejs.org), a MongoDB connection (local or [Atlas](https://www.mongodb.com/cloud/atlas)), and VS Code.

### 1. Backend

```bash
cd backend/SmartLibrary.Api
dotnet restore
```

Copy your MongoDB connection string into `appsettings.Development.json`
(create it if it isn't there — see `database/README.md`), then:

```bash
dotnet run
```

The API starts at **http://localhost:5000** with Swagger UI at
**http://localhost:5000/swagger**.

Default admin login (change in `appsettings.json` → `AdminCredentials`):
```
username: admin
password: Admin@123
```

### 2. Database

Run the one-time index setup — see [`database/README.md`](database/README.md).

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Opens at **http://localhost:5173**.

Full step-by-step walkthrough: [`docs/SETUP.md`](docs/SETUP.md).
API reference: [`docs/API.md`](docs/API.md).

## Features

The app has two entirely separate account types (see the role-selection
screen on first load):

**Librarian (Admin)** — logs in, then can:
- Search the student directory and check students in / out of the physical
  library (the only way attendance ever changes)
- Add walk-in students who haven't registered online yet
- View today's attendance log and search attendance history
- Manage maximum library capacity
- Manage the book catalog — add, search, edit, and delete titles, with
  total/available copy counts and a "New" badge on recent additions
  (`/admin/books`)
- View analytics (peak/quiet hours, occupancy trends)

**Student** — registers their own account (name, index number, password),
then can:
- View live occupancy and available seats from anywhere (phone, laptop)
- View analytics / "best time to visit" recommendations
- **Cannot** check themselves in or out — that's deliberately librarian-only,
  since it reflects who's physically in the building

## Design notes

White background, green accent, with a small deliberate pop of yellow (the
navbar's brand mark, and the "New" badge on recently-added books) rather
than a third base color competing with green for attention.

The dashboard's occupancy indicator is a literal grid of seats (see
`SeatGrid.jsx`) rather than a generic progress bar — each dot represents one
seat and fills in as students check in. Color vocabulary is consistent
throughout: sage green = available, amber = filling up, coral = full.
