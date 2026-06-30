using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class IncomeSource : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IncomeType IncomeType { get; set; }
    public decimal Amount { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EmployerName { get; set; }
    public Guid? DepositAccountId { get; set; }
    public decimal? TaxRate { get; set; }
    public bool IsGross { get; set; } = true; // Gross or net amount
    public string? Notes { get; set; }
    public int? PayDay { get; set; }

    public decimal MonthlyAmount => Frequency switch
    {
        RecurrenceFrequency.Weekly => Amount * 52 / 12,
        RecurrenceFrequency.BiWeekly => Amount * 26 / 12,
        RecurrenceFrequency.SemiMonthly => Amount * 2,
        RecurrenceFrequency.Monthly => Amount,
        RecurrenceFrequency.Quarterly => Amount / 3,
        RecurrenceFrequency.SemiAnnual => Amount / 6,
        RecurrenceFrequency.Annual => Amount / 12,
        _ => Amount
    };

    public decimal AnnualAmount => MonthlyAmount * 12;

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Account? DepositAccount { get; set; }
}
