using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByCategoryAsync(Guid userId, Guid categoryId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByTypeAsync(Guid userId, TransactionType type, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> SearchAsync(Guid userId, string searchTerm, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetSpendingByCategoryAsync(Guid userId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetMonthlySpendingTrendAsync(Guid userId, int months, CancellationToken cancellationToken = default);
    Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
