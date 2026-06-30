# Frontend Guide — React ClientApp

**Location:** `src/Presentation/FinanceManager.Web/ClientApp/`  
**Stack:** React 18, Vite 5, TanStack Query v5, Zustand v5, Tailwind CSS 3, Recharts, react-hook-form

---

## Folder Structure

```
ClientApp/
├── src/
│   ├── services/          ← All API call functions (axios-based)
│   ├── stores/            ← Zustand global state stores
│   ├── pages/             ← One folder per route/feature
│   │   ├── Auth/
│   │   ├── Dashboard/
│   │   ├── Accounts/
│   │   ├── Transactions/
│   │   ├── Budgets/
│   │   ├── Savings/
│   │   ├── Loans/
│   │   ├── Investments/
│   │   ├── Reports/
│   │   └── Settings/
│   ├── components/        ← Reusable UI components
│   ├── hooks/             ← Custom React hooks
│   └── utils/             ← Helpers (currency formatting, dates)
├── index.html
├── vite.config.js         ← Proxy: /api → http://localhost:5106
├── tailwind.config.js
└── package.json
```

---

## Data Flow

```
React Page
  └── useQuery / useMutation (TanStack Query)
        └── service function (e.g. accountsService.getAll())
              └── axios (src/services/api.js)
                    ├── Request interceptor: adds Authorization: Bearer <token>
                    ├── Response interceptor: auto-refresh on 401 using refresh token
                    └── /api/* → Vite proxy → http://localhost:5106
```

---

## `src/services/api.js` — Base Axios Instance

The central axios client used by all service files.

**Base URL:** `/api` (proxied by Vite to `http://localhost:5106`)

**Request interceptor:**
- Reads `token` from `authStore`
- Adds `Authorization: Bearer <token>` header to every request

**Response interceptor (auto-refresh):**
- On `401 Unauthorized`:
  1. Calls `POST /api/auth/refresh` with stored `refreshToken`
  2. If successful: updates `authStore` with new tokens, retries the original request
  3. If refresh fails: clears auth store, redirects to `/login`

---

## Services Reference

### `authService.js`

| Function | Method + Route | Description |
|---|---|---|
| `register(data)` | `POST /auth/register` | `{firstName, lastName, email, password, confirmPassword}` |
| `login(data)` | `POST /auth/login` | `{email, password}` → returns `{accessToken, refreshToken, user}` |
| `refreshToken(token)` | `POST /auth/refresh` | Exchanges refresh token for new access token |
| `logout()` | `POST /auth/logout` | Revokes server-side refresh token |
| `getProfile()` | `GET /auth/profile` | Returns current user profile |
| `updateProfile(data)` | `PUT /auth/profile` | Update name, currency, timezone |

---

### `accountsService.js`

| Function | Method + Route | Description |
|---|---|---|
| `getAll()` | `GET /accounts` | All accounts with credit card / loan / investment summaries |
| `getById(id)` | `GET /accounts/{id}` | Single account |
| `create(data)` | `POST /accounts` | Create new account |
| `update(id, data)` | `PUT /accounts/{id}` | Update account details |
| `delete(id)` | `DELETE /accounts/{id}` | Soft-delete |
| `getSummary()` | `GET /accounts/summary` | Net worth summary |

---

### `transactionsService.js`

| Function | Method + Route | Description |
|---|---|---|
| `getAll(filters)` | `GET /transactions` | Paginated; accepts `TransactionFilterDto` as query params |
| `getById(id)` | `GET /transactions/{id}` | Single transaction |
| `create(data)` | `POST /transactions` | Create transaction |
| `update(id, data)` | `PUT /transactions/{id}` | Update transaction |
| `delete(id)` | `DELETE /transactions/{id}` | Soft-delete |
| `getSummary(year, month)` | `GET /transactions/summary` | Monthly income/expense summary |

---

### `budgetsService.js`

| Function | Method + Route |
|---|---|
| `getAll()` | `GET /budgets` |
| `getById(id)` | `GET /budgets/{id}` |
| `create(data)` | `POST /budgets` |
| `update(id, data)` | `PUT /budgets/{id}` |
| `delete(id)` | `DELETE /budgets/{id}` |

---

### `savingsService.js`

| Function | Method + Route |
|---|---|
| `getAll()` | `GET /savings-goals` |
| `getById(id)` | `GET /savings-goals/{id}` |
| `create(data)` | `POST /savings-goals` |
| `update(id, data)` | `PUT /savings-goals/{id}` |
| `delete(id)` | `DELETE /savings-goals/{id}` |
| `getContributions(id)` | `GET /savings-goals/{id}/contributions` |
| `addContribution(id, data)` | `POST /savings-goals/{id}/contributions` |

---

### `dashboardService.js`

| Function | Method + Route |
|---|---|
| `getDashboard()` | `GET /dashboard` |

---

### `reportsService.js`

| Function | Method + Route |
|---|---|
| `exportPdf(params)` | `GET /reports/export/pdf` |
| `exportExcel(params)` | `GET /reports/export/excel` |

Both return a `Blob` for file download.

---

## Zustand Stores — `src/stores/`

### `authStore.js`

Manages authentication state. Persisted to `localStorage`.

| State | Type | Description |
|---|---|---|
| `user` | `object \| null` | User profile from `AuthResponseDto.User` |
| `token` | `string \| null` | JWT access token |
| `refreshToken` | `string \| null` | Refresh token |
| `isAuthenticated` | `bool` | Derived: `token !== null` |

**Actions:**

| Action | Description |
|---|---|
| `setAuth(user, token, refreshToken)` | Called after successful login/register/refresh |
| `clearAuth()` | Called on logout or failed refresh — clears all state and localStorage |
| `updateUser(user)` | Called after profile update |

---

### `uiStore.js`

Manages global UI state (sidebar, modals, notifications).

| State | Description |
|---|---|
| `sidebarOpen` | Whether the sidebar is expanded |
| `activeModal` | Name of the currently open modal (e.g., `"addTransaction"`) |
| `toasts` | Array of toast notification objects |

**Actions:** `toggleSidebar()`, `openModal(name)`, `closeModal()`, `addToast(message, type)`, `removeToast(id)`

---

## Pages Reference

### `Auth/LoginPage.jsx`
- Form: `email`, `password`
- On submit: calls `authService.login()`, stores tokens via `authStore.setAuth()`
- Redirects to `/dashboard` on success
- Links to `/register`

### `Auth/RegisterPage.jsx`
- Form: `firstName`, `lastName`, `email`, `password`, `confirmPassword`
- On submit: calls `authService.register()`, auto-logs in
- Links to `/login`

### `Dashboard/DashboardPage.jsx`
- Uses `useQuery` with `dashboardService.getDashboard()`
- Displays: net worth card, account balances, recent transactions table, budget progress bars, savings goal circles, monthly spending chart (Recharts `AreaChart`), category breakdown (Recharts `PieChart`)

### `Accounts/AccountsPage.jsx`
- Lists all accounts grouped by type (Bank, Credit Card, Loan, Investment, Cash)
- Add/Edit/Delete modals with react-hook-form
- Shows credit limit / loan balance / investment value per card

### `Transactions/TransactionsPage.jsx`
- Paginated transactions table with search/filter bar
- Filters: account, category, date range, transaction type, amount range
- Inline edit and delete
- "Add Transaction" modal

### `Budgets/BudgetsPage.jsx`
- Month/year picker to switch budgets
- Progress bars per category (Allocated vs Spent)
- Color coding: green (<70%), yellow (70–90%), red (>90% / over budget)

### `Savings/SavingsPage.jsx`
- Goal cards with circular progress indicator
- Add contribution / withdraw modal
- Shows days remaining and auto-contribute schedule

### `Loans/LoansPage.jsx`
- Lists loan accounts with balance, interest rate, monthly payment
- Payoff timeline chart

### `Investments/InvestmentsPage.jsx`
- Portfolio summary (total value, gain/loss)
- Holdings table: symbol, shares, avg cost, current price, unrealized P&L
- Sector allocation chart

### `Reports/ReportsPage.jsx`
- Report type selector
- Date range picker
- Export PDF / Export Excel buttons (triggers file download)

### `Settings/SettingsPage.jsx`
- **Status: Placeholder** — not yet implemented
- Planned: profile settings, currency preferences, notification settings

---

## TanStack Query Patterns

```jsx
// Reading data
const { data, isLoading, error } = useQuery({
  queryKey: ['accounts'],
  queryFn: accountsService.getAll,
});

// Mutating data with cache invalidation
const queryClient = useQueryClient();
const createMutation = useMutation({
  mutationFn: accountsService.create,
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['accounts'] });
    queryClient.invalidateQueries({ queryKey: ['dashboard'] });
  },
});

// Usage
createMutation.mutate({ name: 'BPI Savings', accountType: 2, ... });
```

**Query keys convention:**
- `['accounts']` — all accounts
- `['accounts', id]` — single account
- `['transactions', filters]` — filtered transactions
- `['dashboard']` — dashboard summary
- `['budgets']` / `['budgets', id]`
- `['savings-goals']` / `['savings-goals', id]`

---

## Currency & Number Formatting

All monetary values are formatted using the `Currency` enum value from the API. PHP uses `₱` symbol.

```js
// In utils/currency.js
export const formatCurrency = (amount, currency = 1) => {
  // currency = 1 is PHP
  // Returns: "₱ 50,000.00" or "$ 1,234.56"
};
```

---

## Theme

| Color | Usage |
|---|---|
| Purple (`#7C3AED`) | Primary — buttons, active states, headers |
| Dark (`#0F0F1A`) | Page background |
| Gold (`#F59E0B`) | Accent — highlights, important metrics |
| Green (`#10B981`) | Income, positive values |
| Red (`#EF4444`) | Expenses, negative values, over-budget |
