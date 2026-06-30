using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class InvestmentHolding : BaseEntity
{
    public Guid InvestmentAccountDetailsId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public InvestmentType InvestmentType { get; set; }
    public decimal Shares { get; set; }
    public decimal AverageCostBasis { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal MarketValue => Shares * CurrentPrice;
    public decimal TotalCostBasis => Shares * AverageCostBasis;
    public decimal UnrealizedGainLoss => MarketValue - TotalCostBasis;
    public decimal UnrealizedGainLossPercent => TotalCostBasis > 0 ? (UnrealizedGainLoss / TotalCostBasis) * 100 : 0;
    public decimal RealizedGainLoss { get; set; }
    public DateTime? LastPriceUpdate { get; set; }
    public string? ISIN { get; set; }
    public string? Exchange { get; set; }
    public string? Sector { get; set; }
    public decimal? DividendYield { get; set; }
    public decimal? ExpenseRatio { get; set; }

    public virtual InvestmentAccountDetails InvestmentAccountDetails { get; set; } = null!;
    public virtual ICollection<InvestmentTransaction> Transactions { get; set; } = new List<InvestmentTransaction>();
}
