# Vibe Coding Challenge — DECISIONS.md

This document answers the three questions from the task about my use of
AI tools while building this project. I was honest about what the AI did
and what I did myself, because the point of this section is to show that
I actually understand the code I'm shipping — not that I wrote every line
by hand.

## 1. What did AI generate, and what did I write or modify myself?

I used two AI tools:

- **Z-CODE (powered by GLM-5.2)** as my main assistant for building the
  project — the first version of the API, the Angular app and the tests.
- **Claude** later, for the Clean Architecture refactor after the first
  technical review (with Codex and Grok as independent reviewers — see
  section 4).

I treated them as fast, well-read pair programmers — but I made the
decisions. Concretely:

- **What the AI did:** generated the boilerplate — entity classes, EF Core
  configurations, the DbContext, the service classes, the controllers,
  the Angular components, and the CSS. It saved me a lot of typing.
- **What I did:**
  - **Designed the domain model on paper first** (Artist → Track →
    TrackDistribution → Dsp) and gave the AI the exact fields and
    relationships, so it was implementing my design, not inventing one.
  - **Chose the architecture**: four projects
    (Domain → Application → Infrastructure → API) with dependencies
    pointing only inward, and kept the controllers thin (all logic
    lives in services).
  - **Made sure the code actually works.** As a developer, shipping
    untested code isn't quality work — so I had the AI generate a test
    suite (validators, services, and end-to-end HTTP tests) to verify
    the business rules hold: duplicate ISRCs are rejected, distribution
    auto-promotes a track, auth blocks unauthenticated writes, and so
    on. I pointed it at the scenarios that matter; it wrote the test
    bodies.
  - **The security rules** (JWT on every write endpoint, locking CORS
    to the Angular origin) were my call — the AI's first draft had no
    auth at all.

So AI did most of the typing, but I did all of the design, the security
review, and the debugging when generated code did not compile.

## 2. What security issues did you find in the AI-generated code?

A few real ones:

1. **No auth on write endpoints.** The first version of the API let
   anyone create tracks or trigger distributions with a simple POST — no
   login required. I added JWT bearer authentication and an Admin role,
   so now `POST`/`PATCH`/distribute all require a valid token, while
   `GET` (browsing) stays public.

2. **Password checked with plain `==`.** The login code compared the
   password using `==`, which returns faster or slower depending on how
   much of the password matched. An attacker could measure response times
   to guess the password piece by piece. I replaced it with
   `CryptographicOperations.FixedTimeEquals`, which always takes the same
   time regardless of the input.

3. **CORS open to any website.** The AI suggested `AllowAnyOrigin()`,
   meaning any site on the internet could call the API from a user's
   browser. I restricted it to just the Angular dev origin
   (`http://localhost:4200`).

## 3. One thing the AI got wrong that you had to fix

**The JWT signing key was hardcoded in `appsettings.json`.**

The JWT signing key is the secret used to sign every authentication
token — think of it as the stamp that proves a token genuinely came from
our API. If someone learns that key, they can forge tokens, pretend to
be the admin, and call every protected endpoint (create tracks,
distribute, change statuses) without any credentials.

When the AI wired up JWT authentication, it dropped the signing key
straight into `appsettings.json`:

```json
"Jwt": {
    "Key": "MusicDistributionDevSecretKey1234567890"
}
```

That file is committed to the repository, so the secret is visible to
anyone who reads the source on GitHub — exactly the people you don't
want knowing it.

**What I did about it:** the base `appsettings.json` now ships with an
empty key and empty admin credentials. A *public demo* key and the
`admin` / `admin123` account live only in `appsettings.Development.json`
— still committed, deliberately, so `dotnet run` works out of the box for
the reviewer; I treat them as known-public test values, not secrets. Any other
environment must supply `Jwt__Key` / `Auth__*` from environment variables,
User Secrets or a secret manager. The API validates the options at startup
and refuses to boot if the key is missing or shorter than 32 bytes, so a
misconfigured deployment fails loudly instead of silently signing tokens
with a known key. The lesson: the AI treated a secret like ordinary
configuration, because from the code's perspective a key is "just a
string." Knowing *where a value comes from* matters as much as knowing
what it does.

## 4. Follow-up after review: the Clean Architecture was only skin-deep

After the first technical review I was told the architecture had a
problem, and I went back to check it properly. The project *references*
pointed the right way (Api → Infrastructure → Application → Domain), but
the *contents* did not:

- All use-case logic — ISRC uniqueness, the Draft → Submitted promotion,
  the duplicate-DSP rule, status changes, DTO mapping, login — lived in
  `Infrastructure/Services`, talking straight to `AppDbContext`. The
  Application project only held interfaces, DTOs and validators, so
  swapping EF Core would have meant rewriting the business behaviour.
- `Program.cs` reached into Infrastructure's `AppDbContext` and seeder,
  and duplicated the JWT settings that `AuthService` read by string keys.
- The controller parsed the `status` filter itself and returned a
  different error shape than the rest of the API.

What I changed:

- **Domain** now owns the rules: `Track.DistributeTo(...)` and
  `Track.ChangeStatus(...)` throw a `DomainException` when a rule is broken.
  `Status` has a private setter and `Distributions` is a read-only view over
  a private list, so nothing outside the entity can bypass them. A new track
  always starts as Draft and the status only moves forward
  (Draft → Submitted → Distributed), never past Draft without a DSP submission.
- **Application** owns the use cases (`TrackService`, `ArtistService`,
  `DspService`, `AuthService`) and declares the ports it needs:
  repositories, `IUnitOfWork`, `IIdentityService`, `IJwtTokenGenerator`.
  It has no reference to EF Core, ASP.NET Core or JWT libraries.
- **Infrastructure** only implements those ports (EF repositories, unit
  of work that turns unique-index races into 409s — only unique/primary-key
  violations, other constraint failures stay 500 —, JWT generator,
  credential check, migrations, seeding).
- **Api** is just the composition root: thin controllers, one
  `IExceptionHandler` that turns every failure into ProblemDetails,
  and bearer validation built from the same `JwtSettings` options.

I also fixed a port mismatch (the API listened on 5003 while the Angular
proxy and README used 5080), tightened the ISRC rule to the real
12-character format, rejected numeric status strings, added
`GET /api/dsps` so the UI stops hard-coding DSP ids, and moved the
service tests onto in-memory SQLite so unique indexes are really
exercised (53 tests now). The reviewers also caught a real frontend bug:
an expired token left in `localStorage` crashed the Angular app on
startup, because the auth service touched its `isLoggedIn` signal before
the signal existed — fixed and verified in the browser. I used AI (Claude, with Codex and Grok as independent
reviewers) to help with this refactor, and reviewed and ran every change
myself.
