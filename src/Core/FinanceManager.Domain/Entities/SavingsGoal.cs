using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class SavingsGoal : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? LinkedAccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateOnly? TargetDate { get; set; }
    public RecurrenceFrequency? AutoContributeFrequency { get; set; }
    public decimal? AutoContributeAmount { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public bool IsPaused { get; set; } = false;
    public int Priority { get; set; } = 1;

    public decimal ProgressPercentage => TargetAmount > 0 ? Math.Min((CurrentAmount / TargetAmount) * 100, 100) : 0;
    public decimal RemainingAmount => Math.Max(TargetAmount - CurrentAmount, 0);
    public int? DaysRemaining => TargetDate.HasValue ? (TargetDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days : null;

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Account? LinkedAccount { get; set; }
    public virtual ICollection<SavingsContribution> Contributions { get; set; } = new List<SavingsContribution>();
}
