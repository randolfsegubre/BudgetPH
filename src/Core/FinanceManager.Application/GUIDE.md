# Application Layer Guide — `FinanceManager.Application`

This layer contains all use-case logic: DTOs (data transfer objects), request/response contracts, AutoMapper mappings, and FluentValidation validators. It depends only on the Domain layer.

---

## What Lives Here

| Folder | Contents |
|---|---|
| `Features/{Feature}/DTOs/` | Request and response data contracts |
| `Mappings/MappingProfile.cs` | All AutoMapper source→destination mappings |
| `Features/{Feature}/Validators/` | FluentValidation rules for create/update requests |

> **Note:** MediatR is registered as a dependency but controllers currently query `ApplicationDbContext` directly. The Application layer's primary role is DTOs, mapping, and validation.

---

## DTOs Reference

### Auth DTOs — `AuthDtos.cs`

| DTO | Direction | Key Fields |
|---|---|---|
| `RegisterDto` | Request | `FirstName`, `LastName`, `Email`, `Password`, `ConfirmPassword`, `PhoneNumber?` |
| `LoginDto` | Request | `Email`, `Password`, `RememberMe` (default false) |
| `AuthResponseDto` | Response | `AccessToken`, `RefreshToken`, `ExpiresAt`, `User` (`UserProfileDto`) |
| `UserProfileDto` | Response | `Id`, `FirstName`, `LastName`, `Email`, `PhoneNumber?`, `AvatarUrl?`, `PreferredCurrency`, `TimeZone` |
| `RefreshTokenDto` | Request | `RefreshToken` |
| `ChangePasswordDto` | Request | `CurrentPassword`, `NewPassword`, `ConfirmNewPassword` |
| `UpdateProfileDto` | Request | `FirstName`, `LastName`, `PhoneNumber?`, `PreferredCurrency`, `TimeZone` |

---

### Account DTOs — `AccountDtos.cs`

#### `AccountDto` — full account response

```
Id, Name, AccountNumber?, AccountType, Balance, Currency,
InstitutionName?, InstitutionLogoUrl?, IsActive, IsLinked,
Color?, Icon?, IncludeInNetWorth, CreatedAt,
CreditCard?: CreditCardSummaryDto,
Loan?: LoanSummaryDto,
Investment?: InvestmentSummaryDto
```

> `CreditCard`, `Loan`, and `Investment` are `null` unless the `AccountType` matches.

#### `CreditCardSummaryDto`
`CreditLimit`, `AvailableCredit`, `APR`, `StatementClosingDay`, `PaymentDueDay`, `CardNetwork?`, `Last4Digits?`, `ExpirationDate?`, `AnnualFee`

#### `LoanSummaryDto`
`LoanType`, `OriginalAmount`, `OutstandingBalance`, `InterestRate`, `LoanTermMonths`, `StartDate`, `EndDate?`, `MonthlyPayment`, `LenderName?`, `LoanNumber?`, `PropertyAddress?`, `PropertyValue?`, `PaymentsMade`, `RemainingPayments`, `TotalInterestPaid`

#### `InvestmentSummaryDto`
`InvestmentAccountType`, `BrokerName?`, `TotalCostBasis`, `TotalMarketValue`, `TotalGainLoss`, `TotalGainLossPercent`, `Holdings: List<HoldingDto>`

#### `HoldingDto`
`Id`, `Symbol`, `Name`, `InvestmentType`, `Shares`, `AverageCostBasis`, `CurrentPrice`, `MarketValue`, `TotalCostBasis`, `UnrealizedGainLoss`, `UnrealizedGainLossPercent`, `RealizedGainLoss`, `Sector?`

#### `CreateAccountDto` (Request)
`Name`, `AccountNumber?`, `AccountType`, `OpeningBalance`, `Currency`, `InstitutionName?`, `OpeningDate?`, `Color?`, `Icon?`, `IncludeInNetWorth`

#### `UpdateAccountDto` (Request)
`Name`, `AccountNumber?`, `InstitutionName?`, `IsActive`, `Color?`, `Icon?`, `IncludeInNetWorth`, `Notes?`

---

### Transaction DTOs — `TransactionDtos.cs`

#### `TransactionDto` — response
`Id`, `AccountId`, `AccountName`, `CategoryId?`, `CategoryName?`, `CategoryIcon?`, `CategoryColor?`, `Amount`, `TransactionType`, `Status`, `Description`, `Merchant?`, `TransactionDate`, `IsRecurring`, `Tags?`, `Notes?`, `ReceiptUrl?`

#### `CreateTransactionDto` — request
`AccountId`, `CategoryId?`, `Amount`, `TransactionType`, `Description`, `Merchant?`, `TransactionDate`, `TransferToAccountId?`, `IsRecurring`, `Tags?`, `Notes?`

#### `UpdateTransactionDto` — request
`CategoryId?`, `Amount`, `Description`, `Merchant?`, `TransactionDate`, `Status`, `Tags?`, `Notes?`

#### `TransactionFilterDto` — query params
`AccountId?`, `CategoryId?`, `FromDate?`, `ToDate?`, `Type?`, `Status?`, `MinAmount?`, `MaxAmount?`, `SearchTerm?`, `PageNumber=1`, `PageSize=20`, `SortBy="TransactionDate"`, `SortDescending=true`

#### `MonthlySummaryDto` — response
`Year`, `Month`, `TotalIncome`, `TotalExpenses`, `NetCashFlow`, `TopCategories: List<CategorySpendingDto>`

#### `CategorySpendingDto`
`CategoryId`, `CategoryName`, `CategoryIcon?`, `CategoryColor?`, `Amount`, `TransactionCount`, `Percentage`

---

### Budget DTOs — `BudgetDtos.cs`

#### `BudgetDto` — response
`Id`, `Name`, `Month`, `Year`, `TotalIncomeGoal`, `TotalExpenseLimit`, `TotalAllocated`, `TotalSpent`, `RemainingBudget`, `OverallProgress`, `Items: List<BudgetItemDto>`

#### `BudgetItemDto`
`Id`, `CategoryId`, `CategoryName`, `CategoryIcon?`, `CategoryColor?`, `AllocatedAmount`, `SpentAmount`, `RemainingAmount`, `PercentageUsed`, `IsOverBudget`

#### `CreateBudgetDto` — request
`Name`, `Month`, `Year`, `TotalIncomeGoal`, `TotalExpenseLimit`, `Items: List<CreateBudgetItemDto>`

#### `CreateBudgetItemDto`
`CategoryId`, `AllocatedAmount`

#### `UpdateBudgetItemDto`
`BudgetItemId`, `AllocatedAmount`

---

### Dashboard DTOs — `DashboardDtos.cs`

#### `DashboardSummaryDto` — main response for `/api/dashboard`

| Field | Type | Description |
|---|---|---|
| `TotalNetWorth` | `decimal` | Assets - Liabilities |
| `TotalAssets` | `decimal` | All positive-balance accounts |
| `TotalLiabilities` | `decimal` | Loans + credit card balances |
| `MonthlyIncome` | `decimal` | Current month income |
| `MonthlyExpenses` | `decimal` | Current month expenses |
| `MonthlySavingsRate` | `decimal` | `(Income - Expenses) / Income * 100` |
| `NetCashFlow` | `decimal` | `Income - Expenses` |
| `Accounts` | `List<AccountBalanceSummaryDto>` | All accounts snapshot |
| `RecentTransactions` | `List<RecentTransactionDto>` | Last 10 transactions |
| `BudgetProgress` | `List<BudgetProgressDto>` | Current month budget items |
| `SavingsGoals` | `List<SavingsGoalProgressDto>` | Active goals |
| `UpcomingBills` | `List<UpcomingBillDto>` | Next 30 days |
| `NetWorthTrend` | `NetWorthTrendDto` | 12-month net worth chart data |
| `SpendingTrend` | `List<SpendingByMonthDto>` | 6-month income vs expense |
| `TopSpendingCategories` | `List<CategoryBreakdownDto>` | Top 5 expense categories |

---

### Savings Goal DTOs — `SavingsGoalDtos.cs`

| DTO | Fields |
|---|---|
| `SavingsGoalDto` | `Id`, `Name`, `Description?`, `Icon?`, `Color?`, `TargetAmount`, `CurrentAmount`, `ProgressPercentage`, `RemainingAmount`, `TargetDate?`, `DaysRemaining?`, `IsCompleted`, `IsPaused`, `Priority`, `AutoContributeFrequency?`, `AutoContributeAmount?` |
| `CreateSavingsGoalDto` | `Name`, `Description?`, `TargetAmount`, `TargetDate?`, `LinkedAccountId?`, `Icon?`, `Color?`, `Priority`, `AutoContributeFrequency?`, `AutoContributeAmount?` |
| `SavingsContributionDto` | `Id`, `Amount`, `ContributionDate`, `Notes?`, `IsWithdrawal` |
| `AddContributionDto` | `Amount`, `ContributionDate`, `Notes?`, `IsWithdrawal` |

---

## AutoMapper Mappings — `MappingProfile.cs`

| Source | Destination | Special Mappings |
|---|---|---|
| `Account` | `AccountDto` | `CreditCard ← CreditCardDetails`, `Loan ← LoanDetails`, `Investment ← InvestmentAccountDetails` |
| `CreditCardDetails` | `CreditCardSummaryDto` | `AvailableCredit = CreditLimit + Account.Balance` |
| `LoanDetails` | `LoanSummaryDto` | Direct property match |
| `InvestmentAccountDetails` | `InvestmentSummaryDto` | `Holdings ← Holdings` |
| `InvestmentHolding` | `HoldingDto` | Direct property match |
| `Transaction` | `TransactionDto` | `AccountName ← Account.Name`, `CategoryName/Icon/Color ← Category.*` |
| `Transaction` | `RecentTransactionDto` | Same + `Type ← TransactionType` |
| `Budget` | `BudgetDto` | `TotalAllocated/TotalSpent/RemainingBudget/OverallProgress` — computed from domain entity |
| `BudgetItem` | `BudgetItemDto` | `CategoryName/Icon/Color ← Category.*` |
| `Account` | `AccountBalanceSummaryDto` | Direct |
| `SavingsGoal` | `SavingsGoalProgressDto` | `GoalId ← Id`, `Percentage ← ProgressPercentage` |

### Important: Navigation properties must be loaded

AutoMapper mappings that reference navigation properties (e.g., `Account.Name` on a transaction) require EF to have loaded those properties first. In controllers, always use `.Include()` when the mapping needs related data:

```csharp
// ✅ Correct — loads Category for mapping
var transactions = await context.Transactions
    .Include(t => t.Account)
    .Include(t => t.Category)
    .Where(t => t.UserId == appUser.Id)
    .ToListAsync(ct);

// ❌ Will produce null CategoryName in DTO
var transactions = await context.Transactions
    .Where(t => t.UserId == appUser.Id)
    .ToListAsync(ct);
```
