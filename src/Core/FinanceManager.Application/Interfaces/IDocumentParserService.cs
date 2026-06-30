using FinanceManager.Application.Common.Models;

namespace FinanceManager.Application.Interfaces;

public interface IDocumentParserService
{
    Task<Result<ParsedDocumentData>> ParseDocumentAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
}

public class ParsedDocumentData
{
    public decimal? OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public List<ParsedTransaction> Transactions { get; set; } = [];
}

public class ParsedTransaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsDebit { get; set; }
    public string? Reference { get; set; }
}
