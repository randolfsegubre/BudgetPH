using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class InvestmentAccountDetails : BaseEntity
{
    public Guid AccountId { get; set; }
    public InvestmentAccountType InvestmentAccountType { get; set; }
    public string? BrokerName { get; set; }
    public decimal TotalCostBasis { get; set; }
    public decimal TotalMarketValue { get; set; }
    public decimal TotalGainLoss => TotalMarketValue - TotalCostBasis;
    public decimal TotalGainLossPercent => TotalCostBasis > 0 ? (TotalGainLoss / TotalCostBasis) * 100 : 0;
    public decimal? AnnualContributionLimit { get; set; }
    public decimal YearToDateContributions { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual ICollection<InvestmentHolding> Holdings { get; set; } = new List<InvestmentHolding>();
}
