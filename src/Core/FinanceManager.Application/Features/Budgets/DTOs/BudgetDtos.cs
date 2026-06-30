using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.Features.Budgets.DTOs;

public record BudgetDto(
    Guid Id,
    string Name,
    int Month,
    int Year,
    decimal TotalIncomeGoal,
    decimal TotalExpenseLimit,
    decimal TotalAllocated,
    decimal TotalSpent,
    decimal RemainingBudget,
    decimal OverallProgress,
    List<BudgetItemDto> Items
);

public record BudgetItemDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string? CategoryIcon,
    string? CategoryColor,
    decimal AllocatedAmount,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal PercentageUsed,
    bool IsOverBudget
);

public record CreateBudgetDto(
    string Name,
    int Month,
    int Year,
    decimal TotalIncomeGoal,
    decimal TotalExpenseLimit,
    List<CreateBudgetItemDto> Items
);

public record CreateBudgetItemDto(
    Guid CategoryId,
    decimal AllocatedAmount
);

public record UpdateBudgetItemDto(
    Guid BudgetItemId,
    decimal AllocatedAmount
);
