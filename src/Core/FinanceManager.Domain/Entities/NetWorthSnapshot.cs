using FinanceManager.Domain.Common;

namespace FinanceManager.Domain.Entities;

public class NetWorthSnapshot : BaseEntity
{
    public Guid UserId { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth => TotalAssets - TotalLiabilities;
    public decimal? CashAndBankAssets { get; set; }
    public decimal? InvestmentAssets { get; set; }
    public decimal? RealEstateAssets { get; set; }
    public decimal? OtherAssets { get; set; }
    public decimal? CreditCardLiabilities { get; set; }
    public decimal? LoanLiabilities { get; set; }
    public decimal? MortgageLiabilities { get; set; }
    public decimal? OtherLiabilities { get; set; }
    public string? Notes { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}
