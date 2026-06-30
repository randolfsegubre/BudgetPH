using FinanceManager.Domain.Common;

namespace FinanceManager.Domain.Entities;

public class BudgetItem : BaseEntity
{
    public Guid BudgetId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsRolledOver { get; set; } = false;
    public decimal? RolloverAmount { get; set; }

    public decimal RemainingAmount => AllocatedAmount - SpentAmount;
    public decimal PercentageUsed => AllocatedAmount > 0 ? (SpentAmount / AllocatedAmount) * 100 : 0;
    public bool IsOverBudget => SpentAmount > AllocatedAmount;

    public virtual Budget Budget { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}
