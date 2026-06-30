using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class LoanPaymentSchedule : BaseEntity
{
    public Guid LoanDetailsId { get; set; }
    public int PaymentNumber { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal BalanceAfterPayment { get; set; }
    public bool IsPaid { get; set; } = false;
    public DateTime? PaidOn { get; set; }
    public decimal? ActualAmountPaid { get; set; }

    public virtual LoanDetails LoanDetails { get; set; } = null!;
}
