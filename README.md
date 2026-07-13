# 🎵 Music Distribution — Track Management

A full-stack application for managing music **artists**, **tracks**, and their **distribution** to digital service providers (Spotify, Apple Music, YouTube).

Built as a Takwene hiring task using **vibe coding** (AI-assisted development). See [`DECISIONS.md`](./DECISIONS.md) for the vibe coding write-up.

---

## 🚀 HOW TO RUN THE PROJECT

This section is the quick-start. You need two terminals: one for the
backend API and one for the Angular frontend. **Start the backend
first** — the frontend proxies API calls to it.

### Prerequisites

| Tool | Version | Check |
|------|---------|-------|
| .NET SDK | 10.0+ | `dotnet --version` |
| Node.js | 20+ | `node --version` |
| npm | 10+ | `npm --version` |

### Step 1 — Run the backend (API)

```bash
cd backend
dotnet restore
dotnet run --project src/MusicDistribution.Api
```

On first launch it automatically:
1. Applies EF Core migrations (creates a local `musicdistribution.db`),
2. Seeds sample data (4 artists, 10 tracks, 3 DSPs, 8 distributions).

The API listens on `https://localhost:7000` and `http://localhost:5080`.
The frontend talks to port **5080** via a dev proxy.

### Step 2 — Run the frontend (Angular)

```bash
cd frontend
npm install
npm start        # or: npx ng serve
```

Open **http://localhost:4200** in your browser. You should see the track
list populated with seeded data.

### Step 3 — Log in as admin (to use write endpoints)

Click **Admin login** in the top bar, or go to `/login`:

| Field | Value |
|-------|-------|
| Username | `admin` |
| Password | `admin123` |

After logging in you can create tracks, change statuses, and distribute
tracks to DSPs. Browse (GET) works without login.

### Step 4 — Run the tests (optional)

```bash
cd backend
dotnet test
# Expected: Passed: 28, Failed: 0
```

### Quick troubleshooting

- **Frontend shows empty list / errors:** the backend isn't running or
  isn't on port 5080. Start it first and re-check.
- **Port already in use:** stop anything on 5080 (backend) or 4200
  (frontend), then retry.
- **Want a fresh database:** delete `backend/src/MusicDistribution.Api/musicdistribution.db*`
  — it re-creates and re-seeds on next launch.

---

## 📁 Project structure

```
music-distribution/
├── backend/     .NET 10 Web API (clean architecture)
├── frontend/    Angular 20 standalone-components app
├── README.md    ← you are here
└── DECISIONS.md vibe coding challenge answers
```

## 🧱 Tech stack

| Layer       | Technology                                                        |
|-------------|-------------------------------------------------------------------|
| Backend     | .NET 10, ASP.NET Core, EF Core (SQLite), FluentValidation, JWT    |
| Frontend    | Angular 20 (standalone components, signals), TypeScript           |
| Testing     | xUnit, FluentValidation.TestHelper, WebApplicationFactory (28 tests) |

## ✨ Features

- **Artists** — list (public), create (admin)
- **Tracks** — list with filters (artistId, genre, status), detail view, create, update status
- **Distribution** — submit a track to one or more DSPs; tracks a Draft→Submitted→Distributed lifecycle
- **Auth** — JWT bearer authentication; all write endpoints require an Admin role
- **Validation** — FluentValidation on every input with RFC 7807 ProblemDetails error responses
- **Business rules** — unique ISRC, no duplicate DSP submissions, auto-promote Draft→Submitted on distribution
- **Automated tests** — 28 unit + integration tests covering validators, services, and the HTTP pipeline

---

## 🚀 Backend

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (`dotnet --version` → 10.x)
- `dotnet-ef` CLI tool (optional, for new migrations):
  ```bash
  dotnet tool install --global dotnet-ef
  ```

### Run

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/MusicDistribution.Api
```

The API listens on `https://localhost:7000` / `http://localhost:5080` (check console output).

On startup it:
1. Applies pending EF Core migrations
2. Seeds reference data (3 DSPs, 4 artists, 10 tracks, 8 distributions)

OpenAPI spec is available in Development at `/openapi/v1.json`.

### Architecture (Clean Architecture)

```
src/
├── MusicDistribution.Domain           ← entities + enums (no dependencies)
├── MusicDistribution.Application      ← DTOs, interfaces, validators, exceptions
├── MusicDistribution.Infrastructure   ← EF Core DbContext, services, seeding, migrations
└── MusicDistribution.Api              ← controllers, middleware, DI wiring, Program.cs
```

Dependency flow: `Domain ← Application ← Infrastructure ← Api`

### API endpoints

| Method | Route                        | Auth  | Description                          |
|--------|------------------------------|-------|--------------------------------------|
| POST   | `/api/auth/login`            | —     | Returns a JWT (admin / admin123)     |
| GET    | `/api/artists`               | —     | List all artists                     |
| POST   | `/api/artists`               | Admin | Create an artist                     |
| GET    | `/api/tracks`                | —     | List tracks (filters: artistId, genre, status) |
| GET    | `/api/tracks/{id}`           | —     | Track detail with distributions      |
| POST   | `/api/tracks`                | Admin | Create a track                       |
| PATCH  | `/api/tracks/{id}/status`    | Admin | Update track status                  |
| POST   | `/api/tracks/{id}/distribute`| Admin | Distribute track to DSPs             |

### Run tests

```bash
cd backend
dotnet test
```

### Migrations

```bash
dotnet ef migrations add YourMigrationName \
  --project src/MusicDistribution.Infrastructure \
  --startup-project src/MusicDistribution.Api

dotnet ef database update \
  --project src/MusicDistribution.Infrastructure \
  --startup-project src/MusicDistribution.Api
```

---

## 🖥️ Frontend

### Prerequisites

- Node.js 20+ and npm

### Run

```bash
cd frontend
npm install
npm start        # or: npx ng serve
```

The app runs at `http://localhost:4200`. The dev server proxies `/api` requests to the backend at `http://localhost:5080` (see `proxy.conf.json`), so **start the backend first**.

### Features

- **Track list** (`/`) — sortable table with status filter buttons (All / Draft / Submitted / Distributed)
- **Track detail** (`/tracks/:id`) — full metadata + DSP distribution table; admins can distribute to remaining DSPs via clickable chips
- **Login** (`/login`) — obtains a JWT stored in localStorage; the nav bar shows an "Admin" badge when authenticated
- **JWT interceptor** — automatically attaches the bearer token to every `/api/` request

### Default test credentials

| Field    | Value      |
|----------|------------|
| username | `admin`    |
| password | `admin123` |

---

## 🔌 Obtaining a JWT (curl)

```bash
curl -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Response:
```json
{ "token": "<jwt>", "expiresAt": "2026-..." }
```

Use it on protected endpoints:
```bash
curl -X POST https://localhost:7000/api/tracks \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"title":"New Track","artistId":1,"isrc":"USRC12345678","releaseDate":"2025-01-01","genre":"Pop"}'
```

---

## ⚙️ Configuration

Backend config lives in `backend/src/MusicDistribution.Api/appsettings.json`:

- **ConnectionStrings:DefaultConnection** — SQLite file path (`Data Source=musicdistribution.db`)
- **Jwt** — signing key, issuer, audience (rotate the key for production!)
- **Auth** — default admin credentials

> ⚠️ The default JWT key and admin password are for local development only. Use User Secrets or environment variables in production.
