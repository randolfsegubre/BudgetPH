using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class CreditCardDetails : BaseEntity
{
    public Guid AccountId { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal AvailableCredit => CreditLimit + Account.Balance; // Balance is negative for credit
    public decimal APR { get; set; }
    public int StatementClosingDay { get; set; }
    public int PaymentDueDay { get; set; }
    public decimal MinimumPaymentPercentage { get; set; } = 2;
    public string? CardNetwork { get; set; } // Visa, Mastercard, Amex, Discover
    public string? Last4Digits { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public string? RewardsProgramName { get; set; }
    public decimal RewardsBalance { get; set; }
    public decimal CashAdvanceAPR { get; set; }
    public decimal PenaltyAPR { get; set; }
    public decimal AnnualFee { get; set; }

    public virtual Account Account { get; set; } = null!;
}
