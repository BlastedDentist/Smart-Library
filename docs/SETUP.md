# First-Run Setup Guide (VS Code)

This walks through getting the whole project running locally, step by step,
assuming you're starting from a fresh clone/download.

## 1. Install prerequisites

- **.NET 8 SDK** — check with `dotnet --version` (should print `8.x.x`)
- **Node.js 18+** — check with `node --version`
- **MongoDB** — either install locally, or create a free [Atlas](https://www.mongodb.com/cloud/atlas) cluster (recommended, no local service to manage)
- **VS Code** with the extensions recommended in `.vscode/extensions.json` (VS Code will prompt you to install them when you open the folder — accept it)

## 2. Open the project

```bash
code SmartLibrary
```

Open an integrated terminal (`` Ctrl+` `` / `` Cmd+` ``) — you'll run backend
and frontend commands from two separate terminal tabs, since both need to
run at the same time during development.

## 3. Configure the backend

Create `backend/SmartLibrary.Api/appsettings.Development.json` if it doesn't
already exist, and put your real MongoDB connection string there — this file
is gitignored, so secrets never get committed:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://<user>:<password>@<cluster>.mongodb.net/"
  },
  "JwtSettings": {
    "Secret": "change-this-to-a-long-random-string-at-least-32-characters"
  }
}
```

ASP.NET Core automatically layers `appsettings.Development.json` on top of
`appsettings.json` when `ASPNETCORE_ENVIRONMENT=Development` (the default for
`dotnet run` — see `Properties/launchSettings.json`), so you only need to
override the values that differ from the base file.

## 4. Set up MongoDB indexes

Follow [`../database/README.md`](../database/README.md) — run the index
script once via `mongosh` or MongoDB Compass.

## 5. Run the backend

```bash
cd backend/SmartLibrary.Api
dotnet restore
dotnet run
```

You should see console output ending with something like
`Now listening on: http://localhost:5000`. Visit
**http://localhost:5000/swagger** to confirm the API is up and see every
endpoint documented and testable.

## 6. Run the frontend

In a **second terminal tab**:

```bash
cd frontend
npm install
cp .env.example .env    # only needed if your backend isn't on localhost:5000
npm run dev
```

Visit **http://localhost:5173**. You should see the Dashboard page. If it
shows a connection error instead of occupancy numbers, double check the
backend terminal is still running and `VITE_API_BASE_URL` in `.env` matches
where it's listening.

## 7. Try the flow end-to-end

The app now has two separate account types, so there's a bit more to check:

1. Go to **http://localhost:5173** — you'll land on the role-selection screen.
2. Click **Librarian login**, sign in with the credentials from
   `appsettings.json` (`admin` / `Admin@123` by default). You should land on
   the **Attendance & reports** panel.
3. Under **"Add a walk-in student,"** add a test student (name + index
   number) — this simulates a student who hasn't registered online yet.
4. In the **"Check students in / out"** table, click **Check in** next to
   that student. Their status should flip to "Inside".
5. Log out, go back to the landing page, click **Student login** →
   **Create an account** → register using a *different* index number (or
   the same one you just added as a walk-in, to confirm it attaches the
   password instead of erroring).
6. As the student, you should land on the **Dashboard** and see live
   occupancy (including the seat you just filled as the librarian).
7. Log out, log back in as the librarian, and check that student back out —
   confirm their status flips back and the seat count drops.

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| Frontend shows "Something went wrong" on every page | Backend isn't running, or CORS origin mismatch — check `Cors:AllowedOrigin` in `appsettings.json` matches your Vite port |
| `dotnet run` fails to connect to Mongo | Wrong connection string, or (Atlas) your IP isn't in the Network Access allow-list |
| Admin login always fails | Check `AdminCredentials` in `appsettings.json`/`appsettings.Development.json` |
| Student login says "invalid index number or password" right after registering | Double check you're using the exact same index number, and that registration actually returned success (check the Network tab) |
| Charts show no data | You need at least a few check-ins/check-outs recorded before Analytics has anything to show |
| A student can see the Dashboard but not the Attendance panel | Working as intended — only Admin accounts can check students in/out |
