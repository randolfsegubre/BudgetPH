# BudgetPH — Personal & Family Finance Manager

A full-stack personal finance management web application built for Filipino users and OFWs. Manage accounts, track spending, plan budgets, monitor savings goals, manage loans, and generate financial reports — all in one place with Philippine Peso (PHP) as the primary currency.

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

## Solution Structure

```
FinanceManager/
├── FinanceManager.slnx                  ← Solution file
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
  "Serilog": {
    "MinimumLevel": { "Default": "Debug", "Override": { "Microsoft.EntityFrameworkCore": "Information" } }
  }
}
```

The base `appsettings.json` already has a JWT secret for development. **Do not use the same secret in production.**

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

You need **two terminals running simultaneously**.

**Terminal 1 — API:**

```powershell
dotnet run --project src/Presentation/FinanceManager.API --launch-profile http
# API: http://localhost:5106
# Swagger: http://localhost:5106/swagger
```

**Terminal 2 — Frontend:**

```powershell
cd src/Presentation/FinanceManager.Web/ClientApp
npm run dev
# Frontend: http://localhost:3000
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
| JWT secret in appsettings.json | 🔴 SECURITY | Change before any deployment. Use environment variables in production. |
