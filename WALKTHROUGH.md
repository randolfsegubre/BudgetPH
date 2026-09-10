# Code Walkthrough — FinanceManager (BudgetPH)

This walks through the app by following one real user action — **adding a
transaction** — from the browser click down to the database row, since that
exercises most of the architecture in one pass (auth, EF Core, AutoMapper,
balance/budget side effects). Each stop below names the real file; some of
those files also carry matching `STEP N of M` comments so you can jump
between this doc and the code.

## The shape of the solution

Clean Architecture, four projects, dependencies point inward:

```
src/Presentation/FinanceManager.API/        ASP.NET Core Web API (controllers, Program.cs)
src/Presentation/FinanceManager.Web/        React app (ClientApp/), served as static files by the API
src/Core/FinanceManager.Application/        DTOs + AutoMapper profile — thin, see note below
src/Core/FinanceManager.Domain/             Entities, enums, value objects — no framework dependencies
src/Infrastructure/FinanceManager.Infrastructure/   EF Core DbContext, Identity, repositories, migrations
```

**A real architectural note, not a mistake to "fix":** unlike
`E-Commerse.AI.API` or Lakbay's ABP services, this app has **no MediatR/CQRS
layer**. `FinanceManager.Application` only holds DTOs and the AutoMapper
profile — every controller (`TransactionsController`, `AccountsController`,
etc.) talks to `ApplicationDbContext` directly and contains its own business
logic. That's a "fat controller" style, not broken CQRS — there's no
command/query handler to go looking for, the controller *is* the handler.

## Following "add a transaction" end to end

### 1. `ClientApp/src/pages/Transactions/TransactionsPage.jsx` — the form

The user fills in the New Transaction form and submits. The form's
`onSubmit` calls a React Query mutation (`createMutation.mutate(...)`),
sending `{ ...form, amount: +form.amount, categoryId: form.categoryId || null }`
— note the `+form.amount` (string input coerced to a number) and
`|| null` (an empty category select becomes `null`, not `""`, matching
what the backend's nullable `Guid?` expects).

### 2. `ClientApp/src/services/api.js` — the shared HTTP client

The mutation's `mutationFn` calls through the one shared `api` (axios)
instance every page uses. Two interceptors apply to this request
automatically, with no per-call setup:
- **Request interceptor**: attaches the JWT from `store/authStore.js`.
- **Response interceptor**: if the API answers `401`, tries a token refresh
  once and retries the original request before giving up and logging out.

### 3. `FinanceManager.API/Controllers/TransactionsController.cs` — `Create`

This is where the real work happens (see the method's own `STEP 1 of 5`
comments for the exact sequence): find the account, apply the balance
effect, handle a Transfer's second account, insert the Transaction, roll
the amount into this month's Budget if it's a categorized Expense — then
**one** `SaveChangesAsync` commits everything together. If that call ever
fails, none of the balance/budget changes are persisted either — EF Core's
change tracker is what keeps this atomic without an explicit transaction.

### 4. `FinanceManager.Domain/Entities/Transaction.cs` — the entity being created

Plain data + relationships (`Account`, `Category`, `TransferToAccountId`).
No business logic lives on the entity itself in this app (contrast with
Lakbay's `Product`, which enforces its own invariants via methods like
`UpdatePhysicalProperties`) — the controller owns the rules here.

### 5. `FinanceManager.Infrastructure/Data/ApplicationDbContext.cs` — persistence

EF Core maps `Transaction`/`Account`/`Budget`/etc. to SQL Server tables.
`SaveChangesAsync` is what actually issues the `INSERT`/`UPDATE`
statements for the Transaction row and every Account/BudgetItem the
controller mutated in memory.

### 6. Back to the frontend — cache invalidation

React Query's mutation `onSuccess` invalidates the relevant query keys
(transactions list, account balances, dashboard summary), so every screen
showing that data refetches automatically — no manual "update three
components" bookkeeping.

## Where the real bugs in this PR were, and why

- **`MappingProfile.cs`** (`Account -> AccountDto`): `AccountDto` is a
  `record` (constructor-only, no parameterless constructor). AutoMapper's
  `.ForMember(...)` can only redirect *settable properties* — for a record,
  the actual assignment happens through the constructor, so `ForMember`
  silently did nothing and AutoMapper fell back to
  `Activator.CreateInstance`, which threw. Fixed with `.ForCtorParam(...)`,
  the AutoMapper API that targets a constructor parameter specifically. See
  the file's own comment for the full explanation.
- **`Program.cs`**: `Currency` needed to serialize as a string (`"PHP"`)
  for the frontend, but every other enum in this API is exchanged as its
  numeric value — solved by registering `JsonStringEnumConverter<Currency>()`
  scoped to just that one type, not a blanket string-enum converter that
  would have changed every other enum's wire format too.
- **`TransactionsPage.jsx`**: an empty category select sent `""` to the
  backend instead of `null`, which a nullable `Guid?` can't parse — fixed
  by coercing falsy values to `null` before the request goes out.

## Auth, briefly

JWT bearer auth (`Program.cs`), with a refresh-token flow: `AuthController`
issues both an access token (short-lived) and a refresh token (longer-lived,
stored server-side); `services/api.js`'s response interceptor is what
actually uses the refresh token when the access token expires, transparently
to the rest of the app.
