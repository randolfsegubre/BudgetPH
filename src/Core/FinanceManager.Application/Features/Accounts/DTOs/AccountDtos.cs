using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.Features.Accounts.DTOs;

public record AccountDto(
    Guid Id,
    string Name,
    string? AccountNumber,
    AccountType AccountType,
    decimal Balance,
    Currency Currency,
    string? InstitutionName,
    string? InstitutionLogoUrl,
    bool IsActive,
    bool IsLinked,
    string? Color,
    string? Icon,
    bool IncludeInNetWorth,
    DateTime CreatedAt,
    CreditCardSummaryDto? CreditCard,
    LoanSummaryDto? Loan,
    InvestmentSummaryDto? Investment
);

public record CreditCardSummaryDto(
    decimal CreditLimit,
    decimal AvailableCredit,
    decimal APR,
    int StatementClosingDay,
    int PaymentDueDay,
    string? CardNetwork,
    string? Last4Digits,
    DateOnly? ExpirationDate,
    decimal AnnualFee
);

public record LoanSummaryDto(
    LoanType LoanType,
    decimal OriginalAmount,
    decimal OutstandingBalance,
    decimal InterestRate,
    int LoanTermMonths,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal MonthlyPayment,
    string? LenderName,
    string? LoanNumber,
    string? PropertyAddress,
    decimal? PropertyValue,
    int PaymentsMade,
    int RemainingPayments,
    decimal TotalInterestPaid
);

public record HoldingDto(
    Guid Id,
    string Symbol,
    string Name,
    InvestmentType InvestmentType,
    decimal Shares,
    decimal AverageCostBasis,
    decimal CurrentPrice,
    decimal MarketValue,
    decimal TotalCostBasis,
    decimal UnrealizedGainLoss,
    decimal UnrealizedGainLossPercent,
    decimal RealizedGainLoss,
    string? Sector
);

public record InvestmentSummaryDto(
    InvestmentAccountType InvestmentAccountType,
    string? BrokerName,
    decimal TotalCostBasis,
    decimal TotalMarketValue,
    decimal TotalGainLoss,
    decimal TotalGainLossPercent,
    List<HoldingDto> Holdings
);

public record CreateAccountDto(
    string Name,
    string? AccountNumber,
    AccountType AccountType,
    decimal OpeningBalance,
    Currency Currency,
    string? InstitutionName,
    DateOnly? OpeningDate,
    string? Color,
    string? Icon,
    bool IncludeInNetWorth
);

public record UpdateAccountDto(
    string Name,
    string? AccountNumber,
    string? InstitutionName,
    bool IsActive,
    string? Color,
    string? Icon,
    bool IncludeInNetWorth,
    string? Notes
);
