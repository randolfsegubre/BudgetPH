using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalIncomeGoal { get; set; }
    public decimal TotalExpenseLimit { get; set; }
    public bool IsTemplate { get; set; } = false;
    public bool AutoRollover { get; set; } = false;
    public string? Notes { get; set; }

    public decimal TotalAllocated => Items.Sum(i => i.AllocatedAmount);
    public decimal TotalSpent => Items.Sum(i => i.SpentAmount);
    public decimal RemainingBudget => TotalExpenseLimit - TotalSpent;

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<BudgetItem> Items { get; set; } = new List<BudgetItem>();
}
