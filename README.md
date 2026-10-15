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

### Quickest way — one command (Windows)

From the repo root, double-click **`run.cmd`**, or run:

```powershell
.un.ps1            # start API + frontend, open the browser
.un.ps1 -Fresh     # same, but re-create and re-seed the database first
.un.ps1 -Test      # run the backend tests first
```

It checks .NET/Node, runs `npm install` the first time, starts the API and waits for it,
then starts the frontend and opens http://localhost:4200. Close the two windows to stop.

Or do it by hand:

### Step 1 — Run the backend (API)

```bash
cd backend
dotnet restore
dotnet run --project src/MusicDistribution.Api
```

On first launch it automatically:
1. Applies EF Core migrations (creates a local `musicdistribution.db`),
2. Seeds sample data (4 artists, 10 tracks, 3 DSPs, 10 distributions).

The API listens on `http://localhost:5080` (use `--launch-profile https` to also get `https://localhost:7000`).
The frontend talks to port **5080** via a dev proxy.

Interactive API docs (Development only): **http://localhost:5080/scalar/v1** — log in via
`POST /api/auth/login`, then paste the token into the Bearer auth box.

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

After logging in you can distribute tracks to DSPs from the track detail page.
Creating artists/tracks and changing a track's status are available through the API
(Scalar UI or `backend/src/MusicDistribution.Api/MusicDistribution.Api.http`). Browse (GET) works without login.

### Step 4 — Run the tests (optional)

```bash
cd backend
dotnet test
# Expected: Passed: 53, Failed: 0
```

### Quick troubleshooting

- **Frontend shows empty list / errors:** the backend isn't running or
  isn't on port 5080. Start it first and re-check.
- **Port already in use:** stop anything on 5080 (backend) or 4200
  (frontend), then retry.
- **`Jwt:Key must be at least 32 bytes` on startup:** you are not running in
  `Development` (secrets only live in `appsettings.Development.json`). Use the
  default launch profile, or set `Jwt__Key`, `Auth__Username`, `Auth__Password` env vars.
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
| Testing     | xUnit, FluentValidation.TestHelper, WebApplicationFactory, SQLite in-memory (53 tests) |

## ✨ Features

- **Artists** — list (public), create (admin)
- **Tracks** — list with filters (artistId, genre, status), detail view, create, update status
- **Distribution** — submit a track to one or more DSPs; tracks a Draft→Submitted→Distributed lifecycle
- **Auth** — JWT bearer authentication; all write endpoints require an Admin role
- **Validation** — FluentValidation on every input (body and query string) with RFC 7807 ProblemDetails error responses
- **Business rules** — on the `Track` entity: new tracks start as Draft, distributing promotes Draft→Submitted, no duplicate DSP submissions, status only moves forward (Draft→Submitted→Distributed) and never past Draft without a DSP submission. Unique ISRC / artist email: application pre-check backed by unique DB indexes (races return 409)
- **Automated tests** — 53 domain, application, validator and HTTP integration tests

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

The API listens on `http://localhost:5080` (profile `https` adds `https://localhost:7000`).

On startup (any environment except Production) it:
1. Applies pending EF Core migrations
2. Seeds sample data (3 DSPs, 4 artists, 10 tracks, 10 distributions)

In Development the OpenAPI document is at `/openapi/v1.json` and the Scalar UI at `/scalar/v1`.

### Architecture (Clean Architecture)

```
src/
├── MusicDistribution.Domain           ← entities, enums, business rules (Track.DistributeTo / ChangeStatus), DomainException
├── MusicDistribution.Application      ← use cases (TrackService, ArtistService, DspService, AuthService),
│                                        DTOs, validators, mappings, and the ports it needs:
│                                        IxxxRepository, IUnitOfWork, IIdentityService, IJwtTokenGenerator
├── MusicDistribution.Infrastructure   ← adapters: EF Core DbContext, configurations, migrations, repositories,
│                                        UnitOfWork, seeding, JWT generator, credential check
└── MusicDistribution.Api              ← composition root: controllers (HTTP only), ProblemDetails exception
                                         handler, JWT bearer validation, CORS, OpenAPI
```

Dependency flow: `Api → Infrastructure → Application → Domain` (every arrow points inward).
Application has **no** reference to EF Core, ASP.NET Core or JWT libraries — it only knows its own
interfaces, which Infrastructure implements. Controllers only bind the request and pick the status code.

### API endpoints

| Method | Route                        | Auth  | Description                          |
|--------|------------------------------|-------|--------------------------------------|
| POST   | `/api/auth/login`            | —     | Returns a JWT (admin / admin123)     |
| GET    | `/api/artists`               | —     | List all artists                     |
| GET    | `/api/artists/{id}`          | —     | Get one artist                       |
| POST   | `/api/artists`               | Admin | Create an artist                     |
| GET    | `/api/dsps`                  | —     | List DSPs (used by the distribute UI) |
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

Errors always come back as `application/problem+json`:
400 validation (with per-field `errors`), 401 not logged in / bad credentials,
403 wrong role, 404 not found, 409 conflict / business-rule violation.

### Migrations

The initial migration is already in `src/MusicDistribution.Infrastructure/Migrations` and is applied
automatically on startup. To apply it manually or add a new one:

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
- **Track detail** (`/tracks/:id`) — full metadata + DSP distribution table; admins can distribute to remaining DSPs (loaded from `GET /api/dsps`)
- **Login** (`/login`) — obtains a JWT stored in localStorage; the nav bar shows an "Admin" badge when authenticated
- **JWT interceptor** — attaches the bearer token to every `/api/` request; an expired token or a 401 logs the user out

### Default test credentials

| Field    | Value      |
|----------|------------|
| username | `admin`    |
| password | `admin123` |

---

## 🔌 Obtaining a JWT (curl)

```bash
curl -X POST http://localhost:5080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

Response:
```json
{ "token": "<jwt>", "expiresAt": "2026-..." }
```

Use it on protected endpoints:
```bash
curl -X POST http://localhost:5080/api/tracks \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"title":"New Track","artistId":1,"isrc":"USRC12345678","releaseDate":"2025-01-01","genre":"Pop"}'
```

---

## ⚙️ Configuration

Backend config lives in `backend/src/MusicDistribution.Api/appsettings.json`:

- **ConnectionStrings:DefaultConnection** — SQLite file path (`Data Source=musicdistribution.db`)
- **Jwt** — issuer, audience, expiry. The signing **key** is empty here.
- **Auth** — admin credentials, empty here.
- **Cors:AllowedOrigins** — allowed frontend origins.

The JWT key and the `admin` / `admin123` account in `appsettings.Development.json` are **public demo
values for local Development only** — they are committed on purpose so the reviewer can run the app
with zero setup, and must never be used for a reachable deployment.
In any other environment provide them as environment variables or User Secrets
(`Jwt__Key`, `Auth__Username`, `Auth__Password`); the API refuses to start if the key is missing
or shorter than 32 bytes.
