# API Layer Guide — `FinanceManager.API`

This is the presentation layer. It handles HTTP, authentication, and request routing. Controllers are thin — they resolve the current user, query the database, and return mapped DTOs.

---

## Configuration & Startup — `Program.cs`

| Concern | Detail |
|---|---|
| Logging | Serilog — structured console output + rolling file at `logs/api-*.txt` |
| Authentication | JWT Bearer — validates issuer, audience, lifetime, and signing key from `JwtSettings` config |
| Authorization | `[Authorize]` on all controllers except Auth endpoints |
| Swagger | Available at `/swagger` — includes Bearer token input |
| CORS | `AllowWeb` policy → allows `localhost:3000`, `localhost:5173`, and `WebAppUrl` from config |
| Startup tasks | Auto-migrates DB → runs `DbSeeder.SeedAsync` on first run |
| JSON serialization | camelCase property names (e.g., C# `AccountType` → JSON `"accountType"`) |

### Launch Profiles

| Profile | URL | Notes |
|---|---|---|
| `http` | `http://localhost:5106` | **Use this for local dev** — no HTTPS certificate issues |
| `https` | `https://localhost:7106` | Requires dev certificate |

---

## `BaseApiController` — `Controllers/BaseApiController.cs`

All controllers inherit from this. Never instantiate directly.

**Route:** `[Route("api/[controller]")]`  
**Produces:** `application/json`  
**Auth:** `[Authorize]` applied by default

### `CurrentUserId` property

```csharp
protected string CurrentUserId
```

Reads the `sub` claim (or `ClaimTypes.NameIdentifier`) from the JWT. Throws `UnauthorizedException` if the claim is missing. This is the ASP.NET Core Identity `IdentityUser.Id` (a `string` GUID).

### `GetAppUserAsync` pattern

Each controller defines a private method to convert `CurrentUserId` to an `ApplicationUser`:

```csharp
private async Task<ApplicationUser?> GetAppUserAsync(CancellationToken ct)
    => await context.AppUsers
        .FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
```

Always call this at the start of every action to scope all queries to the current user.

### `HandleResult<T>` helpers

```csharp
protected IActionResult HandleResult<T>(Result<T> result)
// Returns: 200 OK with value, or 400 BadRequest with error message

protected IActionResult HandleResult(Result result)
// Returns: 200 OK, or 400 BadRequest
```

---

## All API Endpoints

### `AuthController` — `/api/auth`

| Method | Route | Auth Required | Description |
|---|---|---|---|
| `POST` | `/register` | No | Register new user; returns `AuthResponseDto` |
| `POST` | `/login` | No | Login; returns `AuthResponseDto` with JWT + refresh token |
| `POST` | `/refresh` | No | Exchange refresh token for new access token |
| `POST` | `/logout` | Yes | Revoke refresh token |
| `GET` | `/profile` | Yes | Get current user's profile |
| `PUT` | `/profile` | Yes | Update name, phone, currency, timezone |
| `POST` | `/change-password` | Yes | Change password |

**JWT structure:**
```
Header.Payload.Signature
Payload claims: sub (IdentityUser.Id), email, name, jti (unique ID)
Expiry: 60 minutes (default)
Refresh token: 30 days
```

---

### `AccountsController` — `/api/accounts`

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | Get all accounts for current user (with credit card, loan, investment summaries) |
| `GET` | `/{id}` | Get single account by ID |
| `POST` | `/` | Create account |
| `PUT` | `/{id}` | Update account |
| `DELETE` | `/{id}` | Soft-delete account |
| `GET` | `/summary` | Get net worth summary (total assets, liabilities, breakdown by account type) |

**Response shape (`AccountDto`):**
```json
{
  "id": "...",
  "name": "BPI Savings Account",
  "accountType": 2,
  "balance": 50000.00,
  "currency": 1,
  "creditCard": null,
  "loan": null,
  "investment": null
}
```

---

### `TransactionsController` — `/api/transactions`

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | Get paginated transactions (supports query filters) |
| `GET` | `/{id}` | Get single transaction |
| `POST` | `/` | Create transaction (updates account balance automatically) |
| `PUT` | `/{id}` | Update transaction |
| `DELETE` | `/{id}` | Soft-delete transaction |
| `GET` | `/summary` | Get monthly income/expense summary with category breakdown |

**Query parameters for `GET /`:**
```
?accountId=&categoryId=&fromDate=&toDate=
&type=&status=&minAmount=&maxAmount=&searchTerm=
&pageNumber=1&pageSize=20&sortBy=TransactionDate&sortDescending=true
```

---

### `BudgetsController` — `/api/budgets`

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | Get all budgets for current user |
| `GET` | `/{id}` | Get budget with all items and category details |
| `POST` | `/` | Create monthly budget with budget items |
| `PUT` | `/{id}` | Update budget (name, goals, limits) |
| `DELETE` | `/{id}` | Delete budget |

---

### `DashboardController` — `/api/dashboard`

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | Get full dashboard summary (`DashboardSummaryDto`) |

Returns a single large response containing: net worth, accounts, recent transactions, budget progress, savings goals, upcoming bills, net worth trend (12 months), spending trend (6 months), top spending categories.

---

### `SavingsGoalsController` — `/api/savings-goals`

| Method | Route | Description |
|---|---|---|
| `GET` | `/` | List all savings goals |
| `GET` | `/{id}` | Get single goal with contributions |
| `POST` | `/` | Create savings goal |
| `PUT` | `/{id}` | Update goal |
| `DELETE` | `/{id}` | Delete goal |
| `GET` | `/{id}/contributions` | List contributions for a goal |
| `POST` | `/{id}/contributions` | Add contribution or withdrawal |

---

### `ReportsController` — `/api/reports`

| Method | Route | Description |
|---|---|---|
| `GET` | `/export/pdf` | Export financial report as PDF (QuestPDF) |
| `GET` | `/export/excel` | Export financial report as Excel (ClosedXML) |

**Query parameters:** `reportType`, `fromDate`, `toDate`, `accountId?`

Returns a file download (`application/pdf` or `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`).

---

## Adding a New Controller — Pattern

```csharp
[ApiController]
public class MyNewController(ApplicationDbContext context, IMapper mapper)
    : BaseApiController
{
    private async Task<ApplicationUser?> GetAppUserAsync(CancellationToken ct)
        => await context.AppUsers
            .FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser is null) return Unauthorized();

        var items = await context.MyEntities
            .Where(e => e.UserId == appUser.Id)
            .ToListAsync(ct);

        return Ok(mapper.Map<List<MyDto>>(items));
    }
}
```

---

## Error Responses

| Scenario | Status Code |
|---|---|
| Invalid JWT / expired token | `401 Unauthorized` |
| Resource not found | `404 Not Found` |
| Validation failure (FluentValidation) | `400 Bad Request` with error details |
| Unhandled exception | `500 Internal Server Error` (details in dev, opaque in prod) |

---

## Development Tips

- **Swagger UI** at `http://localhost:5106/swagger` — click **Authorize**, enter `Bearer <your_token>` to test protected endpoints
- Register a user via `POST /api/auth/register`, login via `POST /api/auth/login`, copy the `accessToken` from the response
- **Logs** are written to `src/Presentation/FinanceManager.API/logs/api-<date>.txt` — useful for debugging EF queries
- Set `"Microsoft.EntityFrameworkCore": "Information"` in `appsettings.Development.json` to see generated SQL in the console
