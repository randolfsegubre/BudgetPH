using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.Features.Transactions.DTOs;

public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string AccountName,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryIcon,
    string? CategoryColor,
    decimal Amount,
    TransactionType TransactionType,
    TransactionStatus Status,
    string Description,
    string? Merchant,
    DateTime TransactionDate,
    bool IsRecurring,
    string? Tags,
    string? Notes,
    string? ReceiptUrl
);

public record CreateTransactionDto(
    Guid AccountId,
    Guid? CategoryId,
    decimal Amount,
    TransactionType TransactionType,
    string Description,
    string? Merchant,
    DateTime TransactionDate,
    Guid? TransferToAccountId,
    bool IsRecurring,
    string? Tags,
    string? Notes
);

public record UpdateTransactionDto(
    Guid? CategoryId,
    decimal Amount,
    string Description,
    string? Merchant,
    DateTime TransactionDate,
    TransactionStatus Status,
    string? Tags,
    string? Notes
);

public record TransactionFilterDto(
    Guid? AccountId,
    Guid? CategoryId,
    DateTime? FromDate,
    DateTime? ToDate,
    TransactionType? Type,
    TransactionStatus? Status,
    decimal? MinAmount,
    decimal? MaxAmount,
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 20,
    string SortBy = "TransactionDate",
    bool SortDescending = true
);

public record MonthlySummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetCashFlow,
    List<CategorySpendingDto> TopCategories
);

public record CategorySpendingDto(
    Guid CategoryId,
    string CategoryName,
    string? CategoryIcon,
    string? CategoryColor,
    decimal Amount,
    int TransactionCount,
    decimal Percentage
);
