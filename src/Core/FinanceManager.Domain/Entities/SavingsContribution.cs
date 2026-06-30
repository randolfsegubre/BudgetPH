using FinanceManager.Domain.Common;

namespace FinanceManager.Domain.Entities;

public class SavingsContribution : BaseEntity
{
    public Guid SavingsGoalId { get; set; }
    public decimal Amount { get; set; }
    public DateTime ContributionDate { get; set; }
    public string? Notes { get; set; }
    public bool IsWithdrawal { get; set; } = false;
    public Guid? TransactionId { get; set; }

    public virtual SavingsGoal SavingsGoal { get; set; } = null!;
}
