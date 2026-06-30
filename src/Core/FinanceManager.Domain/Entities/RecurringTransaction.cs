using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class RecurringTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Merchant { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly NextDueDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoPost { get; set; } = false;
    public int? DayOfMonth { get; set; }
    public int? DayOfWeek { get; set; }
    public string? Notes { get; set; }
    public int TotalOccurrences { get; set; }
    public int? MaxOccurrences { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Account Account { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
