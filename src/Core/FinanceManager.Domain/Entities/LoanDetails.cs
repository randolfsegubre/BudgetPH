using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class LoanDetails : BaseEntity
{
    public Guid AccountId { get; set; }
    public LoanType LoanType { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTermMonths { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public string? LenderName { get; set; }
    public string? LoanNumber { get; set; }
    public int? PaymentDueDay { get; set; }
    public bool IsFixedRate { get; set; } = true;
    public decimal? OriginationFee { get; set; }
    public string? PropertyAddress { get; set; } // For mortgage
    public decimal? PropertyValue { get; set; }  // For mortgage
    public int PaymentsMade { get; set; }
    public int TotalPayments => LoanTermMonths;
    public int RemainingPayments => TotalPayments - PaymentsMade;
    public decimal TotalInterestPaid { get; set; }
    public decimal TotalPrincipalPaid { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual ICollection<LoanPaymentSchedule> PaymentSchedule { get; set; } = new List<LoanPaymentSchedule>();
}
