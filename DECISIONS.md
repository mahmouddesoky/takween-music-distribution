# Vibe Coding Challenge — DECISIONS.md

This document answers the three questions from the task about my use of
AI tools while building this project. I was honest about what the AI did
and what I did myself, because the point of this section is to show that
I actually understand the code I'm shipping — not that I wrote every line
by hand.

## 1. What did AI generate, and what did I write or modify myself?

I used **Z-CODE (powered by GLM-5.2)** as my main AI assistant throughout
the project. I treated it as a fast, well-read pair programmer — but I
made the decisions. Concretely:

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

**What I did about it:** for this hiring task I left the key in place so
the project runs out of the box (no setup steps for the reviewer), but I
flagged it clearly in the README — in a real deployment the key must
come from **User Secrets**, an **environment variable**, or a **secret
manager**, never from a file that lives in version control. The lesson:
the AI treated a secret like ordinary configuration, because from the
code's perspective a key is "just a string." Knowing *where a value
comes from* matters as much as knowing what it does.
