using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TransferToAccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Cleared;
    public string Description { get; set; } = string.Empty;
    public string? Merchant { get; set; }
    public string? MerchantCategory { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ValueDate { get; set; }
    public bool IsRecurring { get; set; } = false;
    public Guid? RecurringTransactionId { get; set; }
    public string? Tags { get; set; } // comma-separated
    public string? Notes { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ImportSource { get; set; }
    public bool IsImported { get; set; } = false;
    public decimal? OriginalAmount { get; set; }
    public string? OriginalCurrency { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual RecurringTransaction? RecurringTransaction { get; set; }
}
