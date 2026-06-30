# Domain Layer Guide — `FinanceManager.Domain`

This layer is the heart of the application. It has **zero dependencies** on other layers. All business rules and invariants live here.

---

## Dependency Rule

```
Domain ← (no dependencies)
     ↑
Application
     ↑
Infrastructure
     ↑
API / Web
```

---

## `BaseEntity` — All Entities Inherit This

**File:** `Entities/BaseEntity.cs`

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Auto-assigned `Guid.NewGuid()` on construction |
| `CreatedAt` | `DateTime` | Set by EF's `SaveChangesAsync` override |
| `UpdatedAt` | `DateTime?` | Updated by EF's `SaveChangesAsync` override |
| `CreatedBy` | `string?` | Optional audit field |
| `UpdatedBy` | `string?` | Optional audit field |
| `IsDeleted` | `bool` | Soft-delete flag — `false` by default |
| `DeletedAt` | `DateTime?` | Set when soft-deleted |
| `DomainEvents` | `IReadOnlyCollection<IDomainEvent>` | Collected events, dispatched after save |

**Soft-delete global query filters** are applied in `ApplicationDbContext` for:
`ApplicationUser`, `Account`, `Transaction`, `Budget`, `SavingsGoal`

Rows marked `IsDeleted = true` are **invisible to all EF queries** on those entities unless you use `IgnoreQueryFilters()`.

---

## Entities

### `ApplicationUser`

**Table:** `fm_AppUsers`  
Represents a registered user profile. Linked to ASP.NET Core Identity via `IdentityUserId`.

| Property | Type | Notes |
|---|---|---|
| `FirstName` | `string` | |
| `LastName` | `string` | |
| `Email` | `string` | |
| `PhoneNumber` | `string?` | |
| `AvatarUrl` | `string?` | |
| `DateOfBirth` | `DateOnly?` | |
| `Gender` | `Gender?` | Enum |
| `TimeZone` | `string?` | Default: `"Asia/Manila"` |
| `PreferredCurrency` | `Currency` | Default: `Currency.PHP` |
| `IdentityUserId` | `string?` | FK to `AspNetUsers.Id` |
| `IsActive` | `bool` | |
| `LastLoginAt` | `DateTime?` | |
| `FullName` | `string` | Computed: `$"{FirstName} {LastName}"` |

**Navigation properties:**

| Property | Type |
|---|---|
| `Accounts` | `ICollection<Account>` |
| `Budgets` | `ICollection<Budget>` |
| `SavingsGoals` | `ICollection<SavingsGoal>` |
| `IncomeSources` | `ICollection<IncomeSource>` |
| `FinancialDocuments` | `ICollection<FinancialDocument>` |
| `RecurringTransactions` | `ICollection<RecurringTransaction>` |
| `NetWorthSnapshots` | `ICollection<NetWorthSnapshot>` |
| `Notifications` | `ICollection<UserNotification>` |

---

### `Account`

**Table:** `fm_Accounts`  
Represents any financial account (bank, credit card, loan, investment, cash, e-wallet).

| Property | Type | Notes |
|---|---|---|
| `UserId` | `Guid` | FK → `ApplicationUser` |
| `Name` | `string` | Display name |
| `AccountNumber` | `string?` | Masked/partial for display |
| `AccountType` | `AccountType` | Enum — drives which sub-details exist |
| `Balance` | `decimal` | Current balance (negative for credit cards) |
| `Currency` | `Currency` | Enum |
| `InstitutionName` | `string?` | Bank/lender name |
| `InstitutionLogoUrl` | `string?` | |
| `Notes` | `string?` | |
| `IsActive` | `bool` | Default: `true` |
| `IsLinked` | `bool` | Whether connected to external bank feed |
| `ExternalAccountId` | `string?` | External bank sync ID |
| `ExternalProvider` | `string?` | e.g. "Plaid" |
| `OpeningBalance` | `decimal` | Balance at account creation |
| `OpeningDate` | `DateOnly?` | |
| `Color` | `string?` | Hex color for UI card |
| `Icon` | `string?` | Icon name/emoji |
| `IncludeInNetWorth` | `bool` | Default: `true` |

**Navigation properties:**

| Property | Type | When present |
|---|---|---|
| `User` | `ApplicationUser` | Always |
| `Transactions` | `ICollection<Transaction>` | Always |
| `CreditCardDetails` | `CreditCardDetails?` | When `AccountType == CreditCard` |
| `LoanDetails` | `LoanDetails?` | When `AccountType == Loan` |
| `InvestmentAccountDetails` | `InvestmentAccountDetails?` | When `AccountType == Investment` |

> **Pattern:** An `Account` is the base record. Type-specific data lives in a 1:1 detail entity (`CreditCardDetails`, `LoanDetails`, `InvestmentAccountDetails`). Always check `AccountType` before accessing these navigation properties.

---

### `Transaction`

**Table:** `fm_Transactions`  
Every money movement — income, expense, transfer, loan payment, investment trade, etc.

| Property | Type | Notes |
|---|---|---|
| `AccountId` | `Guid` | FK → `Account` (source account) |
| `UserId` | `Guid` | FK → `ApplicationUser` |
| `CategoryId` | `Guid?` | FK → `Category` (optional) |
| `TransferToAccountId` | `Guid?` | FK → `Account` (destination for transfers) |
| `RecurringTransactionId` | `Guid?` | FK → `RecurringTransaction` (if auto-generated) |
| `Amount` | `decimal` | Always positive |
| `TransactionType` | `TransactionType` | Enum — defines money direction |
| `Status` | `TransactionStatus` | Default: `Cleared` |
| `Description` | `string` | Required |
| `Merchant` | `string?` | |
| `MerchantCategory` | `string?` | MCC code |
| `Notes` | `string?` | |
| `Tags` | `string?` | Comma-separated |
| `ReceiptUrl` | `string?` | |
| `ReferenceNumber` | `string?` | Bank reference |
| `TransactionDate` | `DateTime` | When money moved |
| `ValueDate` | `DateTime?` | When it settled |
| `IsRecurring` | `bool` | |
| `IsImported` | `bool` | From CSV/bank feed |
| `OriginalAmount` | `decimal?` | In original currency (for FX) |
| `OriginalCurrency` | `string?` | ISO code |
| `ExchangeRate` | `decimal?` | |
| `Latitude` | `decimal?` | Location at time of purchase |
| `Longitude` | `decimal?` | |
| `ImportSource` | `string?` | |

---

### `Budget`

**Table:** `fm_Budgets`  
A monthly spending plan. Contains multiple `BudgetItem` lines, one per category.

| Property | Type | Notes |
|---|---|---|
| `UserId` | `Guid` | |
| `Name` | `string` | e.g. "July 2026 Budget" |
| `Month` | `int` | 1–12 |
| `Year` | `int` | |
| `TotalIncomeGoal` | `decimal` | Target income for the month |
| `TotalExpenseLimit` | `decimal` | Total spending cap |
| `IsTemplate` | `bool` | Can be cloned to future months |
| `AutoRollover` | `bool` | Carries unspent amounts to next month |
| `Notes` | `string?` | |

**Computed properties:**

| Property | Formula |
|---|---|
| `TotalAllocated` | `Items.Sum(i => i.AllocatedAmount)` |
| `TotalSpent` | `Items.Sum(i => i.SpentAmount)` |
| `RemainingBudget` | `TotalExpenseLimit - TotalSpent` |

---

### `BudgetItem`

**Table:** `fm_BudgetItems`  
One category line within a Budget.

| Property | Type | Notes |
|---|---|---|
| `BudgetId` | `Guid` | FK → `Budget` |
| `CategoryId` | `Guid` | FK → `Category` |
| `AllocatedAmount` | `decimal` | Budgeted amount |
| `SpentAmount` | `decimal` | Actual spend (updated from transactions) |
| `IsRolledOver` | `bool` | |
| `RolloverAmount` | `decimal?` | Amount carried over from prior month |
| `Notes` | `string?` | |

**Computed:**

| Property | Formula |
|---|---|
| `RemainingAmount` | `AllocatedAmount - SpentAmount` |
| `PercentageUsed` | `SpentAmount / AllocatedAmount * 100` |
| `IsOverBudget` | `SpentAmount > AllocatedAmount` |

---

### `Category`

**Table:** `fm_Categories`  
Hierarchical — a category can have a parent category (subcategories supported).

| Property | Type | Notes |
|---|---|---|
| `Name` | `string` | |
| `Icon` | `string?` | Emoji or icon name |
| `Color` | `string?` | Hex color |
| `Type` | `CategoryType` | `Income`, `Expense`, or `Transfer` |
| `IsSystem` | `bool` | `true` = seeded by system, cannot be deleted by user |
| `UserId` | `Guid?` | `null` = system category; set = user-created custom category |
| `ParentCategoryId` | `Guid?` | FK → self |
| `SortOrder` | `int` | For UI ordering |

**26 system categories are seeded on first run** including: Salary, Freelance, Allowance, Food & Dining, Transportation, Utilities, Housing, Healthcare, Education, Shopping, Entertainment, Investments, Savings, Loan Payment, and more.

---

### `SavingsGoal`

**Table:** `fm_SavingsGoals`

| Property | Type | Notes |
|---|---|---|
| `UserId` | `Guid` | |
| `LinkedAccountId` | `Guid?` | Optional dedicated savings account |
| `Name` | `string` | e.g. "Emergency Fund" |
| `Description` | `string?` | |
| `Icon` | `string?` | |
| `Color` | `string?` | |
| `TargetAmount` | `decimal` | |
| `CurrentAmount` | `decimal` | Updated when contributions are added |
| `TargetDate` | `DateOnly?` | Deadline |
| `IsCompleted` | `bool` | |
| `IsPaused` | `bool` | |
| `CompletedAt` | `DateTime?` | |
| `AutoContributeFrequency` | `RecurrenceFrequency?` | |
| `AutoContributeAmount` | `decimal?` | |
| `Priority` | `int` | 1 = highest |

**Computed:**

| Property | Formula |
|---|---|
| `ProgressPercentage` | `CurrentAmount / TargetAmount * 100` (capped at 100) |
| `RemainingAmount` | `TargetAmount - CurrentAmount` |
| `DaysRemaining` | `(TargetDate - today).TotalDays` |

---

### Type-Specific Account Details

#### `CreditCardDetails`
1:1 with `Account` where `AccountType == CreditCard`.

| Property | Notes |
|---|---|
| `CreditLimit`, `APR`, `CashAdvanceAPR`, `PenaltyAPR` | Decimal rates |
| `AnnualFee`, `RewardsBalance` | |
| `StatementClosingDay`, `PaymentDueDay` | Day of month (1–31) |
| `MinimumPaymentPercentage` | Default: 2% |
| `CardNetwork` | "Visa", "Mastercard", etc. |
| `Last4Digits`, `ExpirationDate` | |
| `RewardsProgramName` | |
| `AvailableCredit` | Computed: `CreditLimit + Account.Balance` (Balance is negative for cards) |

#### `LoanDetails`
1:1 with `Account` where `AccountType == Loan`.

| Property | Notes |
|---|---|
| `LoanType` | Enum: Personal, Auto, Mortgage, Student, Business, Medical, etc. |
| `OriginalAmount`, `OutstandingBalance`, `InterestRate` | |
| `MonthlyPayment`, `LoanTermMonths`, `PaymentsMade` | |
| `StartDate`, `EndDate?` | |
| `LenderName`, `LoanNumber` | |
| `PropertyAddress`, `PropertyValue` | For mortgage |
| `OriginationFee`, `TotalInterestPaid`, `TotalPrincipalPaid` | |
| `IsFixedRate` | `true` = fixed, `false` = variable |
| `TotalPayments` | `LoanTermMonths` |
| `RemainingPayments` | `LoanTermMonths - PaymentsMade` |

#### `InvestmentAccountDetails`
1:1 with `Account` where `AccountType == Investment`.

| Property | Notes |
|---|---|
| `InvestmentAccountType` | Enum: Brokerage, IRA, RothIRA, 401k, etc. |
| `BrokerName` | |
| `TotalCostBasis`, `TotalMarketValue` | |
| `YearToDateContributions`, `AnnualContributionLimit?` | |
| `TotalGainLoss`, `TotalGainLossPercent` | Computed |
| `Holdings` | `ICollection<InvestmentHolding>` |

#### `InvestmentHolding`
Individual security position within an investment account.

| Property | Notes |
|---|---|
| `Symbol`, `Name` | e.g. "APPL", "Apple Inc." |
| `InvestmentType` | Enum: Stock, Bond, ETF, MutualFund, Crypto, etc. |
| `Shares`, `AverageCostBasis`, `CurrentPrice` | |
| `RealizedGainLoss` | From sold shares |
| `DividendYield`, `ExpenseRatio` | Optional |
| `ISIN`, `Exchange`, `Sector` | Optional metadata |
| `MarketValue` | Computed: `Shares * CurrentPrice` |
| `TotalCostBasis` | Computed: `Shares * AverageCostBasis` |
| `UnrealizedGainLoss` | Computed: `MarketValue - TotalCostBasis` |
| `UnrealizedGainLossPercent` | Computed |

---

## Key Enums

All enums are in `FinanceEnums.cs`.

### `AccountType`
`Checking=1`, `Savings=2`, `CreditCard=3`, `Investment=4`, `Loan=5`, `Cash=6`, `Other=7`

### `Currency` (Philippine-first)
| Value | Code | Notes |
|---|---|---|
| 1 | PHP | Philippine Peso — primary |
| 2 | USD | US Dollar |
| 3 | EUR | Euro |
| 4 | GBP | British Pound |
| 16 | SAR | Saudi Riyal (OFW) |
| 17 | AED | UAE Dirham (OFW) |
| 18 | KWD | Kuwaiti Dinar (OFW) |
| 22 | KRW | Korean Won |

### `TransactionType`
`Income=1`, `Expense=2`, `Transfer=3`, `LoanPayment=4`, `InvestmentBuy=5`, `InvestmentSell=6`, `CreditCardPayment=7`, `SavingsDeposit=8`, `Refund=9`, `Fee=10`, `Interest=11`, `Dividend=12`

### `TransactionStatus`
`Pending=1`, `Cleared=2`, `Reconciled=3`, `Cancelled=4`

### `CategoryType`
`Income=1`, `Expense=2`, `Transfer=3`

### `RecurrenceFrequency`
`Daily=1`, `Weekly=2`, `BiWeekly=3`, `SemiMonthly=4`, `Monthly=5`, `Quarterly=6`, `SemiAnnual=7`, `Annual=8`

### `LoanType`
`Personal=1`, `Auto=2`, `Mortgage=3`, `Student=4`, `Business=5`, `Medical=6`, `HomeEquity=7`, `HELOC=8`, `PayDay=9`, `Other=10`

---

## Entity Relationship Diagram

```
ApplicationUser
  ├── Accounts[]
  │     ├── Transactions[]
  │     ├── CreditCardDetails? (1:1)
  │     ├── LoanDetails? (1:1)
  │     └── InvestmentAccountDetails? (1:1)
  │           └── InvestmentHoldings[]
  ├── Budgets[]
  │     └── BudgetItems[]
  │           └── Category (FK)
  ├── SavingsGoals[]
  │     └── SavingsContributions[]
  ├── IncomeSources[]
  ├── RecurringTransactions[]
  ├── FinancialDocuments[]
  ├── NetWorthSnapshots[]
  └── UserNotifications[]

Categories (hierarchical, self-referencing)
  ├── ParentCategory? (FK → self)
  └── SubCategories[]
```
