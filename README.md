# Retainica

Full-stack spaced repetition flashcard app — live at **[retainica.com](https://retainica.com)**.

Retainica schedules reviews with the FSRS algorithm so you study each card at the moment you're about to forget it. Built as a complete, deployed product: auth, study engine, goals, analytics, AI-assisted card creation, and email reminders — running at $0/month across four cloud services.

## Tech Stack

**Backend**
- ASP.NET Core Web API (.NET 10) — 10 resource controllers behind JWT bearer auth
- Entity Framework Core with Npgsql — Postgres (Neon) in production
- Hangfire for background jobs (email reminders), feature-flagged for cold-start control

**Frontend**
- React 19 + TypeScript (Vite), feature-sliced architecture
- Tailwind CSS 4, Radix UI primitives, Zustand for state, React Hook Form + Zod for forms
- Axios API layer with single-flight 401 refresh-and-retry

**Deployment**
- Render (API) · Vercel (frontend) · Neon (Postgres) · Resend (email)

## Features

- **FSRS scheduling** — full state machine (New / Learning / Review / Relearning) with Anki-style learning steps (1m → 10m → 1d) and FSRS-4.5 default weights
- **Auth done properly** — 15-minute access tokens, 30-day rotating refresh tokens with revocation, password reset with single-use expiring tokens, non-blocking email verification, hard-cascade account deletion
- **Study modes** — spaced repetition with Again/Hard/Good/Easy grading, plus Free Study (streaks and analytics without touching scheduling state)
- **AI card generation** — bring-your-own API key, stored AES-256 encrypted
- **Goals** — multi-deck study goals with budget recalculation when decks change
- **Email reminders** — daily and behind-pace notifications via Hangfire + Resend
- **Deck tools** — CRUD, fork, public deck search (paginated), CSV/JSON import and export
- **Analytics** — review history, streaks, and per-deck stats

## Architecture Notes

- **Session integrity** — review logs are written at review time, not session end; abandoned sessions auto-close when the next one starts
- **Crash-safe by design** — no soft deletes; DB-level cascade rules verified end-to-end (account deletion exercises every FK in the schema)
- **Provider-aware model config** — native `text[]` arrays on Postgres with a JSON fallback path for other providers
- **Migrations applied on startup** — deploy is a push, not a runbook

## Repository Layout

```
src/
  Retainica.Api/        # ASP.NET Core Web API
    Controllers/        # Auth, Users, Decks, Cards, Study, Goals, Reminders, Analytics, Ai, Settings
    Services/           # Business logic (FSRS engine, auth, decks, study, ...)
    Models/ DTOs/       # EF entities and request/response contracts
    Jobs/               # Hangfire jobs
  frontend/             # React + TypeScript (Vite)
    src/features/       # auth, decks, cards, study, goals, reminders, ai, analytics, sharing
    src/components/     # layout + shared UI
    src/services/       # API layer, token store
```

## Running Locally

**Backend** (requires .NET 10 SDK and a Postgres instance):

```bash
cd src/Retainica.Api
# set ConnectionStrings:DefaultConnection and Jwt:Key in appsettings.Development.json
dotnet run    # http://localhost:5000, migrations apply automatically
```

**Frontend:**

```bash
cd src/frontend
npm install
npm run dev   # http://localhost:5173, expects VITE_API_BASE_URL in .env
```

## Roadmap

Phase 2+: rich text editing, media uploads (Cloudflare R2), Cloze cards, per-user FSRS weight tuning, Anki import/export, in-app notifications.
