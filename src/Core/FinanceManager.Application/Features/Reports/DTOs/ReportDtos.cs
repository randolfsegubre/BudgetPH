using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.Features.Reports.DTOs;

public record ReportRequestDto(
    ReportType ReportType,
    DateTime FromDate,
    DateTime ToDate,
    string OutputFormat, // "pdf", "excel", "json"
    Guid? AccountId,
    bool IncludeCharts = true
);

public record ReportResultDto(
    string FileName,
    string ContentType,
    byte[] FileContent,
    ReportType ReportType,
    DateTime GeneratedAt
);

public record FinancialSummaryReportDto(
    string PeriodLabel,
    DateTime FromDate,
    DateTime ToDate,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetSavings,
    decimal SavingsRate,
    List<IncomeBreakdownDto> IncomeBreakdown,
    List<ExpenseBreakdownDto> ExpenseBreakdown,
    List<MonthlyTrendDto> MonthlyTrend,
    decimal OpeningNetWorth,
    decimal ClosingNetWorth,
    decimal NetWorthChange
);

public record IncomeBreakdownDto(string Source, decimal Amount, decimal Percentage);
public record ExpenseBreakdownDto(string Category, decimal Amount, decimal Percentage);
public record MonthlyTrendDto(string Month, decimal Income, decimal Expenses, decimal Savings);

public record DebtReportDto(
    decimal TotalDebt,
    decimal TotalMonthlyPayment,
    decimal DebtToIncomeRatio,
    List<DebtDetailDto> Debts,
    List<PayoffProjectionDto> PayoffProjections
);

public record DebtDetailDto(
    string AccountName,
    LoanType LoanType,
    decimal Balance,
    decimal InterestRate,
    decimal MonthlyPayment,
    int RemainingPayments,
    DateOnly? EstimatedPayoffDate,
    decimal TotalInterestRemaining
);

public record PayoffProjectionDto(string Month, decimal TotalBalance);
