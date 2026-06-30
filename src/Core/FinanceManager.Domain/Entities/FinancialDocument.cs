using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class FinancialDocument : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? AccountId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public DateOnly? StatementPeriodStart { get; set; }
    public DateOnly? StatementPeriodEnd { get; set; }
    public bool IsProcessed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }
    public string? ParsedDataJson { get; set; }
    public string? ProcessingError { get; set; }
    public decimal? StatementBalance { get; set; }
    public int? TransactionsFound { get; set; }
    public int? TransactionsImported { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Account? Account { get; set; }
}
