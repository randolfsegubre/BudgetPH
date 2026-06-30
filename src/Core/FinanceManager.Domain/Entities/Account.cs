using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public AccountType AccountType { get; set; }
    public decimal Balance { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public string? InstitutionName { get; set; }
    public string? InstitutionLogoUrl { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsLinked { get; set; } = false;
    public string? ExternalAccountId { get; set; }
    public string? ExternalProvider { get; set; }
    public decimal OpeningBalance { get; set; }
    public DateOnly? OpeningDate { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IncludeInNetWorth { get; set; } = true;

    // Navigation
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual CreditCardDetails? CreditCardDetails { get; set; }
    public virtual LoanDetails? LoanDetails { get; set; }
    public virtual InvestmentAccountDetails? InvestmentAccountDetails { get; set; }
}
