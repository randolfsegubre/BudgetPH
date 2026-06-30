using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.Features.Dashboard.DTOs;

public record DashboardSummaryDto(
    decimal TotalNetWorth,
    decimal TotalAssets,
    decimal TotalLiabilities,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    decimal MonthlySavingsRate,
    decimal NetCashFlow,
    List<AccountBalanceSummaryDto> Accounts,
    List<RecentTransactionDto> RecentTransactions,
    List<BudgetProgressDto> BudgetProgress,
    List<SavingsGoalProgressDto> SavingsGoals,
    List<UpcomingBillDto> UpcomingBills,
    NetWorthTrendDto NetWorthTrend,
    List<SpendingByMonthDto> SpendingTrend,
    List<CategoryBreakdownDto> TopSpendingCategories
);

public record AccountBalanceSummaryDto(
    Guid Id,
    string Name,
    AccountType AccountType,
    decimal Balance,
    Currency Currency,
    string? InstitutionName,
    string? Color,
    string? Icon
);

public record RecentTransactionDto(
    Guid Id,
    string AccountName,
    string Description,
    string? CategoryName,
    string? CategoryIcon,
    string? CategoryColor,
    decimal Amount,
    TransactionType Type,
    DateTime TransactionDate
);

public record BudgetProgressDto(
    Guid BudgetId,
    string CategoryName,
    string? CategoryIcon,
    string? CategoryColor,
    decimal Allocated,
    decimal Spent,
    decimal Percentage,
    bool IsOverBudget
);

public record SavingsGoalProgressDto(
    Guid GoalId,
    string Name,
    string? Icon,
    string? Color,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal Percentage,
    DateOnly? TargetDate,
    int? DaysRemaining
);

public record UpcomingBillDto(
    string Name,
    decimal Amount,
    DateOnly DueDate,
    int DaysUntilDue,
    string? AccountName,
    bool IsOverdue
);

public record NetWorthTrendDto(
    List<string> Labels,
    List<decimal> Values
);

public record SpendingByMonthDto(
    string Month,
    decimal Income,
    decimal Expenses
);

public record CategoryBreakdownDto(
    string CategoryName,
    string? Icon,
    string? Color,
    decimal Amount,
    decimal Percentage
);
