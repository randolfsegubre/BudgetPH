# Infrastructure Layer Guide — `FinanceManager.Infrastructure`

This layer implements data persistence. It depends on Domain and Application layers. No business logic lives here — only EF Core configuration and data seeding.

---

## `ApplicationDbContext`

**File:** `Data/ApplicationDbContext.cs`  
**Inherits:** `IdentityDbContext<IdentityUser>` — ASP.NET Core Identity tables are included automatically.

All Identity tables are prefixed with `fm_` (e.g., `fm_AspNetUsers`, `fm_AspNetRoles`).

---

## DbSets (Tables)

| DbSet | Entity | Table |
|---|---|---|
| `AppUsers` | `ApplicationUser` | `fm_AppUsers` |
| `Accounts` | `Account` | `fm_Accounts` |
| `CreditCardDetails` | `CreditCardDetails` | `fm_CreditCardDetails` |
| `LoanDetails` | `LoanDetails` | `fm_LoanDetails` |
| `LoanPaymentSchedules` | `LoanPaymentSchedule` | `fm_LoanPaymentSchedules` |
| `InvestmentAccountDetails` | `InvestmentAccountDetails` | `fm_InvestmentAccountDetails` |
| `InvestmentHoldings` | `InvestmentHolding` | `fm_InvestmentHoldings` |
| `InvestmentTransactions` | `InvestmentTransaction` | `fm_InvestmentTransactions` |
| `Transactions` | `Transaction` | `fm_Transactions` |
| `RecurringTransactions` | `RecurringTransaction` | `fm_RecurringTransactions` |
| `Categories` | `Category` | `fm_Categories` |
| `Budgets` | `Budget` | `fm_Budgets` |
| `BudgetItems` | `BudgetItem` | `fm_BudgetItems` |
| `SavingsGoals` | `SavingsGoal` | `fm_SavingsGoals` |
| `SavingsContributions` | `SavingsContribution` | `fm_SavingsContributions` |
| `IncomeSources` | `IncomeSource` | `fm_IncomeSources` |
| `FinancialDocuments` | `FinancialDocument` | `fm_FinancialDocuments` |
| `NetWorthSnapshots` | `NetWorthSnapshot` | `fm_NetWorthSnapshots` |
| `UserNotifications` | `UserNotification` | `fm_UserNotifications` |
| `RefreshTokens` | `RefreshToken` | `fm_RefreshTokens` |

---

## `OnModelCreating` Configuration

### Soft-Delete Global Query Filters
Applied to these entities — EF automatically appends `WHERE IsDeleted = 0` to all queries:

```csharp
ApplicationUser  →  entity.HasQueryFilter(u => !u.IsDeleted)
Account          →  entity.HasQueryFilter(a => !a.IsDeleted)
Transaction      →  entity.HasQueryFilter(t => !t.IsDeleted)
Budget           →  entity.HasQueryFilter(b => !b.IsDeleted)
SavingsGoal      →  entity.HasQueryFilter(g => !g.IsDeleted)
```

To query deleted records (e.g., for audit), use:
```csharp
context.Accounts.IgnoreQueryFilters().Where(a => a.IsDeleted)
```

### Cascade Delete Behavior
Most FK relationships use `OnDelete(DeleteBehavior.Restrict)` to prevent SQL Server's multiple-cascade-paths error. This means:
- You must manually delete child records before deleting a parent
- Or use soft-delete (the preferred approach)

Exceptions where `Cascade` is used:
- `BudgetItems` when a `Budget` is deleted (hard cascade — items have no meaning without budget)

### `SaveChangesAsync` Override
Automatically stamps `CreatedAt` and `UpdatedAt` on any `BaseEntity`:

```csharp
// On Insert: sets CreatedAt = UtcNow
// On Update: sets UpdatedAt = UtcNow
```

---

## Seed Data — `DbSeeder`

**File:** `Data/DbSeeder.cs`  
Called at startup in `Program.cs` via `DbSeeder.SeedAsync(scope)`.

Seeds **26 system categories** with `IsSystem = true` and `UserId = null`:

| Category | Type | Icon |
|---|---|---|
| Salary / Wages | Income | 💼 |
| Freelance / Business | Income | 💻 |
| Investments / Dividends | Income | 📈 |
| Allowance / Transfers | Income | 💝 |
| Food & Dining | Expense | 🍔 |
| Transportation | Expense | 🚗 |
| Utilities | Expense | ⚡ |
| Housing / Rent | Expense | 🏠 |
| Healthcare / Medical | Expense | 🏥 |
| Education | Expense | 📚 |
| Shopping / Retail | Expense | 🛍️ |
| Entertainment | Expense | 🎬 |
| Travel | Expense | ✈️ |
| Personal Care | Expense | 💆 |
| Subscriptions | Expense | 📱 |
| Insurance | Expense | 🛡️ |
| Taxes | Expense | 📋 |
| Charitable Giving | Expense | 🤲 |
| Family / Children | Expense | 👨‍👩‍👧 |
| Pet Care | Expense | 🐾 |
| Savings Deposit | Transfer | 💰 |
| Investment Purchase | Transfer | 📊 |
| Loan Payment | Transfer | 🏦 |
| Credit Card Payment | Transfer | 💳 |
| Transfer In | Transfer | ⬇️ |
| Transfer Out | Transfer | ⬆️ |

---

## Running Migrations

```powershell
# From solution root (FinanceManager/)

# Add a migration
dotnet ef migrations add <Name> \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API

# Apply to database
dotnet ef database update \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API

# List migrations and their status
dotnet ef migrations list \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API

# Generate SQL script (for production deployment review)
dotnet ef migrations script \
  --project src/Infrastructure/FinanceManager.Infrastructure \
  --startup-project src/Presentation/FinanceManager.API \
  --output migration.sql
```

---

## Auto-Migration on Startup

`Program.cs` runs pending migrations automatically on startup:

```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await db.Database.MigrateAsync();          // applies pending migrations
await DbSeeder.SeedAsync(scope.ServiceProvider); // seeds categories if empty
```

This means the database is always up-to-date when the API starts — no manual `dotnet ef database update` needed in development.

---

## Adding a New Entity — Checklist

1. Create entity class in `FinanceManager.Domain/Entities/` extending `BaseEntity`
2. Add a DbSet in `ApplicationDbContext`
3. Configure in `OnModelCreating` (table name, constraints, indexes, FK behavior)
4. Add soft-delete global query filter if needed
5. Add DTOs in `FinanceManager.Application/Features/{Feature}/DTOs/`
6. Add AutoMapper mapping in `MappingProfile.cs`
7. Run `dotnet ef migrations add <Name>`
8. Update seed data if needed
