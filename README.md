# BudgetPH — Personal & Family Finance Manager

A full-stack personal finance management web application built for Filipino users and OFWs. Manage accounts, track spending, plan budgets, monitor savings goals, manage loans, and generate financial reports — all in one place with Philippine Peso (PHP) as the primary currency.

---

## Verified State (2026-09-07)

This project was audited end-to-end on 2026-09-07: clean `dotnet build` of the whole solution, backend running locally against a real SQL Server database with migrations applied and system categories seeded, and the React frontend built, run in dev mode, and driven through a real browser session that registered a user, logged in, created accounts, and posted transactions — all round-tripping through the live API (not mocked).

- **Backend**: `dotnet build` succeeds with 0 errors (7 warnings — see Known Issues). `dotnet test` passes (1 domain test, 1 application test — coverage is minimal, see "What's left"). API runs with `dotnet run --project src/Presentation/FinanceManager.API --launch-profile http`, connects to a real database, applies EF Core migrations automatically on startup (`db.Database.MigrateAsync()` in `Program.cs`), and seeds the 26 system categories.
- **Frontend**: `npm install` and `npm run build` both succeed (one "chunk larger than 500kB" warning, cosmetic). `npm run dev` serves the SPA on port 3000, proxying `/api/*` to the API on port 5106 — confirmed working via the browser, not just config inspection.
- **End-to-end confirmed working in a real browser**: register → login → dashboard renders real net worth/account data → Accounts page create/list → Transactions page create (with and without a category) → dashboard updates live from the new transaction. Screenshots taken during the audit showed real PHP/USD-formatted balances, not placeholder data.

### Bugs found and fixed during this audit

| Bug | Symptom | Fix |
|---|---|---|
| `Currency` enum serialized/deserialized as a raw integer | Every "Add Account" request failed with `400 Bad Request` because the frontend sends `currency: "PHP"` (a string) but the API expected a number | Registered `JsonStringEnumConverter<Currency>()` scoped to just the `Currency` type in `Program.cs`, so `Currency` round-trips as `"PHP"`/`"USD"` while every other enum (`AccountType`, `TransactionType`, etc.) keeps its existing numeric contract untouched |
| `AccountDto` (a record) couldn't be constructed by AutoMapper | Every call to `GET /api/accounts`, `GET /api/accounts/{id}`, and `POST /api/accounts` threw `System.ArgumentException: ... needs to have a constructor with 0 args` (HTTP 500) | `CreditCard`/`Loan`/`Investment` ctor params don't line up by name with the entity's `CreditCardDetails`/`LoanDetails`/`InvestmentAccountDetails` navigation properties. `ForMember` silently can't redirect to a record's constructor parameter, so AutoMapper fell back to `Activator.CreateInstance`, which fails for records with no parameterless ctor. Switched to `ForCtorParam` in `MappingProfile.cs`, which is the documented way to target a specific ctor parameter |
| Transaction form sent `categoryId: ""` when "No category" was selected | `POST /api/transactions` failed with `400 Bad Request` ("The JSON value could not be converted ... categoryId") any time a transaction was added without picking a category — a very common case | `TransactionsPage.jsx` now sends `categoryId: form.categoryId || null` on submit instead of the raw empty string |

None of these were large unfinished features — all three were small, mechanical wiring/contract bugs in otherwise complete, working code, and all three were reproduced against the real running app (not guessed from reading source) before being fixed and re-verified.

**Screenshots from the audit** (real seeded test account, not mockups): [`docs/e2e/budgetph-account-added.png`](docs/e2e/budgetph-account-added.png) shows a real BPI Credit Card account created via the UI (exercising the `ForCtorParam` fix above); [`docs/e2e/budgetph-balance-updated.png`](docs/e2e/budgetph-balance-updated.png) shows the same account after posting a ₱250 transaction with no category selected (exercising the `categoryId: null` fix above) — Total Liabilities correctly updated to ₱250.00.

---

## Error handling — E2E verified 2026-09-10

`ErrorBoundary.jsx` (class component, catches render errors — wraps `<QueryClientProvider>` in `main.jsx`) and `NotFoundPage.jsx` (replaces the previous silent `<Navigate to="/" replace />` catch-all, which hid genuinely broken links instead of surfacing them) were added and verified live: a temporary throwing component confirmed the boundary catches the error and shows the fallback UI, and clicking "Back to Home" reloads to the dashboard while the user is still authenticated — a real `window.location.href` reset, not a soft re-render that would hit the same broken state again.

---

## What This Application Does

| Feature | Description |
|---|---|
| **Accounts** | Track bank accounts, credit cards, loans, investments, e-wallets, and cash |
| **Transactions** | Log income, expenses, transfers, and loan payments with category tagging |
| **Budgets** | Create monthly budgets with per-category spending limits and auto-rollover |
| **Savings Goals** | Set and track savings targets with auto-contribution scheduling |
| **Investments** | Monitor portfolio holdings, market value, gains/losses |
| **Loans** | Track loan balances, interest rates, payment schedules |
| **Dashboard** | Real-time net worth, spending trends, cash flow summary, upcoming bills |
| **Reports** | Export financial reports as PDF or Excel |
| **Multi-currency** | PHP primary; supports USD, EUR, SAR, AED, KWD and 15+ other currencies |

**Target users:** Filipino professionals, OFWs managing remittances, families tracking household finances.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Backend framework | ASP.NET Core (.NET 10) |
| Architecture | Clean Architecture (Domain → Application → Infrastructure → Presentation) |
| ORM | EF Core 10 (code-first, SQL Server LocalDB for dev) |
| Auth | ASP.NET Core Identity + JWT Bearer tokens |
| Mediator | MediatR 12.5.0 |
| Validation | FluentValidation 11.12.0 |
| Mapping | AutoMapper 13.0.1 |
| PDF reports | QuestPDF 2026.x |
| Excel reports | ClosedXML |
| API docs | Swashbuckle / Swagger UI |
| Frontend | React 18 + Vite 5 |
| State management | TanStack Query v5 + Zustand v5 |
| Styling | Tailwind CSS 3 |
| Charts | Recharts |
| Forms | react-hook-form |

---

## Architecture

BudgetPH lives in a **single repository** with the API and React frontend co-located. Both run as independent processes during development.

### Solution Structure

```
FinanceManager/
├── FinanceManager.slnx                  ← Solution file (.NET projects only)
├── src/
│   ├── Core/
│   │   ├── FinanceManager.Domain/       ← Entities, enums, domain events (no dependencies)
│   │   └── FinanceManager.Application/ ← DTOs, validators, AutoMapper mappings
│   ├── Infrastructure/
│   │   └── FinanceManager.Infrastructure/ ← EF Core DbContext, migrations, data seeding
│   └── Presentation/
│       ├── FinanceManager.API/          ← ASP.NET Core Web API (port 5106)
│       └── FinanceManager.Web/
│           └── ClientApp/              ← React + Vite SPA (port 3000)
└── tests/
    ├── FinanceManager.Domain.Tests/
    └── FinanceManager.Application.Tests/
```

> `FinanceManager.Web/ClientApp/` is a standalone React/Vite project. It is **not** a .NET project and does not appear in the solution file. It runs independently via `npm run dev`.

Each layer has its own `GUIDE.md` with detailed object, method, and relationship documentation.

---

## Local Development Setup

### Prerequisites

| Tool | Version | Install |
|---|---|---|
| .NET SDK | 10.0.x | https://dotnet.microsoft.com/download |
| Node.js | 20+ LTS | https://nodejs.org |
| SQL Server LocalDB | Included with VS / standalone | https://aka.ms/sqllocaldb |
| Git | Any | https://git-scm.com |

> **Check if LocalDB is installed:** run `SqlLocalDB info` in a terminal. You should see `MSSQLLocalDB` listed.

---

### Step-by-Step First-Time Setup

**1. Clone the repository**

```powershell
git clone https://github.com/randolfsegubre/BudgetPH.git
cd BudgetPH
```

**2. Install .NET dependencies**

```powershell
dotnet restore
```

**3. Install React dependencies**

```powershell
cd src/Presentation/FinanceManager.Web/ClientApp
npm install
cd ../../../..
```

**4. Configure the application**

The API uses `appsettings.Development.json` for local settings. It is git-ignored (contains secrets). Create it at:

```
src/Presentation/FinanceManager.API/appsettings.Development.json
```

With this content:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BudgetPH_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "<generate your own random 32+ character string for local dev>"
  },
  "Serilog": {
    "MinimumLevel": { "Default": "Debug", "Override": { "Microsoft.EntityFrameworkCore": "Information" } }
  }
}
```

> Any reachable SQL Server works, not just LocalDB — this was verified during the 2026-09-07 audit against both a plain `Server=(localdb)\mssqllocaldb` instance and a full local `Server=localhost` SQL Server 2025 install with `TrustServerCertificate=True`. Use whichever you have.

The committed `appsettings.json` intentionally ships a **placeholder** `JwtSettings:SecretKey` (`CHANGE_THIS_...`) rather than a real key — it is not usable as-is. Every developer must set a real value in their own `appsettings.Development.json` (shown above) or via user-secrets / an environment variable. **Never commit a real secret to `appsettings.json`.**

**5. Create the database**

```powershell
dotnet ef database update \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API
```

This creates the `BudgetPH_Dev` database and seeds 26 system categories.

**6. Verify the build**

```powershell
dotnet build
# Expected: Build succeeded. 0 Error(s)
```

---

### Running the Application

You need **two processes running simultaneously**.

**API — Visual Studio 2026 (recommended):**
```
Open FinanceManager.slnx → press F5
Browser opens http://localhost:5106/swagger automatically
```

**API — CLI:**
```powershell
dotnet run --project src/Presentation/FinanceManager.API --launch-profile http
# Swagger UI: http://localhost:5106/swagger
```

**Frontend:**
```powershell
cd src/Presentation/FinanceManager.Web/ClientApp
npm run dev
# UI: http://localhost:3000
```

Then open **http://localhost:3000** in your browser.

> The Vite dev server proxies all `/api/*` requests to `http://localhost:5106` automatically — no CORS issues in development.

---

### EF Core Migration Commands

```powershell
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API

# Apply migrations
dotnet ef database update \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API

# Revert last migration
dotnet ef database update <PreviousMigrationName> \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API
```

---

## Architecture Flow

```
Browser (http://localhost:3000)
  └── React SPA (Vite)
        └── TanStack Query + axios
              └── /api/* → proxied to http://localhost:5106

ASP.NET Core API (http://localhost:5106)
  └── JWT Bearer Middleware
        └── [Authorize] Controllers
              └── BaseApiController (resolves CurrentUserId from JWT)
                    └── ApplicationDbContext (EF Core)
                          └── AutoMapper → DTOs → JSON (camelCase)
                                └── SQL Server LocalDB (BudgetPH_Dev)
```

### Request lifecycle:
1. React page calls a service function (e.g., `accountsService.getAll()`)
2. axios sends `GET /api/accounts` with `Authorization: Bearer <token>` header
3. Vite proxy forwards to `http://localhost:5106/api/accounts`
4. `AccountsController` validates JWT, resolves `CurrentUserId`
5. Controller queries `ApplicationDbContext`, maps to DTOs via AutoMapper
6. Returns JSON; TanStack Query caches the result
7. React component re-renders with data

---

## Layer-by-Layer Developer Guides

| Layer | Guide File |
|---|---|
| Domain (entities, enums, events) | [src/Core/FinanceManager.Domain/GUIDE.md](src/Core/FinanceManager.Domain/GUIDE.md) |
| Application (DTOs, validation, mapping) | [src/Core/FinanceManager.Application/GUIDE.md](src/Core/FinanceManager.Application/GUIDE.md) |
| Infrastructure (database, migrations) | [src/Infrastructure/FinanceManager.Infrastructure/GUIDE.md](src/Infrastructure/FinanceManager.Infrastructure/GUIDE.md) |
| API (controllers, endpoints, auth) | [src/Presentation/FinanceManager.API/GUIDE.md](src/Presentation/FinanceManager.API/GUIDE.md) |
| Frontend (React pages, services, stores) | [src/Presentation/FinanceManager.Web/ClientApp/GUIDE.md](src/Presentation/FinanceManager.Web/ClientApp/GUIDE.md) |

---

## Known Issues

| Issue | Severity | Notes |
|---|---|---|
| AutoMapper 13.0.1 vulnerability GHSA-rvv3-g6hj-g44x | ⚠️ HIGH | Functional. Planned replacement with Mapster. |
| EF decimal precision warnings on startup | ℹ️ INFO | Cosmetic — data is not lost. Fix: add `HasPrecision(18,4)` in `OnModelCreating`. |
| JWT secret must be supplied per-environment | 🔴 SECURITY | `appsettings.json` ships a `CHANGE_THIS_...` placeholder (not a real key) — set a real one via `appsettings.Development.json`/user-secrets locally and via environment variables/secret manager in any deployed environment. |
| No `GET /api/categories`-style admin endpoint to manage custom (non-system) categories | ℹ️ INFO | Reading categories works (`GET /api/transactions/categories`, used by the transaction form); there's no UI/endpoint yet to let a user create their own categories beyond the 26 seeded system ones. |
| `GET /api/transactions/categories` returns raw `Category` entities, not a DTO | ℹ️ INFO | Works correctly (verified serving real data), but leaks internal EF navigation/audit fields (`domainEvents`, `subCategories`, `createdBy`, etc.) instead of going through AutoMapper like other endpoints. Low risk (read-only, no secrets), but inconsistent with the rest of the API — worth a `CategoryDto` pass later. |
| Test coverage is minimal | ⚠️ MEDIUM | Only 1 domain test + 1 application test exist; both pass, but there is no meaningful coverage of controllers, AutoMapper profiles, or the auth flow. The AutoMapper/record bug fixed in this audit (see "Verified State" above) is exactly the kind of regression a mapping-profile unit test would have caught. |
| CORS origin list is hardcoded to dev ports | ℹ️ INFO | `Program.cs` allows `http://localhost:3000`, `http://localhost:5173`, and `WebAppUrl` config. Fine for local dev; a real frontend origin must be added (via `WebAppUrl` or another entry) before deploying. |
| Committed `logs/*.txt` files under `FinanceManager.API/logs/` | ℹ️ INFO | Serilog's rolling file sink output appears to be tracked in git rather than ignored. Harmless but should be added to `.gitignore` at some point to stop local run logs from showing up as diffs. |

---

## Path to a Test/Production Deployment

The app is genuinely dev-complete (see "Verified State" above), but the following still need real decisions/work before it should run anywhere but a developer's machine:

1. **Secrets.** `JwtSettings:SecretKey` and the DB connection string currently live in local, git-ignored `appsettings.Development.json` / SQL LocalDB or a local SQL Server instance. For a deployed environment, move both to actual environment variables or a secret manager (Azure Key Vault, AWS Secrets Manager, etc.) — nothing in `Program.cs` needs to change, it already reads from `IConfiguration`.
2. **A real database server.** LocalDB and a local Windows SQL Server instance are both dev-only. Point `ConnectionStrings:DefaultConnection` at a real hosted SQL Server (Azure SQL, an RDS SQL Server instance, etc.) and re-run `dotnet ef database update` against it — the existing single `InitialCreate` migration applies cleanly (confirmed during this audit).
3. **CORS origins.** Add the real deployed frontend origin(s) to the `AllowWeb` policy in `Program.cs` (currently only `localhost:3000`/`5173` and an optional `WebAppUrl` config value).
4. **HTTPS/hosting.** `Program.cs` calls `UseHttpsRedirection()` but the `http` launch profile only binds an HTTP port — fine for local dev, but a real deployment needs a proper TLS-terminating host (reverse proxy, App Service, container platform, etc.) in front of the API.
5. **Frontend build artifact.** `npm run build` outputs static files to `src/Presentation/FinanceManager.Web/wwwroot` (see `vite.config.js`). Decide whether the API serves these as static files or whether the SPA is deployed separately (e.g. a CDN/static host) with the API's real base URL swapped in for the dev-only Vite proxy.
6. **AutoMapper vulnerability.** GHSA-rvv3-g6hj-g44x (see Known Issues) should be resolved — either an AutoMapper patch/major-version update or the already-planned Mapster migration — before shipping publicly.
7. **Widen test coverage** beyond the current 2 unit tests, especially around the AutoMapper profiles and controllers, given the class of bug this audit found (a working feature silently 500ing/400ing end-to-end with no test to catch it).
8. **Categories DTO cleanup** and **`.gitignore` for `logs/`** (see Known Issues) — small polish items, not blockers.

None of the above were addressed in this audit because they require an environment/infrastructure decision (where to host, which secret manager, which domain) rather than a code fix — they're listed here so the next session (or a hiring manager reading this repo) knows exactly what's stubbed versus what's real.
