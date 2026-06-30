using FinanceManager.Domain.Common;

namespace FinanceManager.Domain.Entities;

public class InvestmentTransaction : BaseEntity
{
    public Guid HoldingId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Buy, Sell, Dividend, Split, Transfer
    public decimal Shares { get; set; }
    public decimal PricePerShare { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Commission { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Notes { get; set; }

    public virtual InvestmentHolding Holding { get; set; } = null!;
}
