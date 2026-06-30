using FinanceManager.Domain.Entities;

namespace FinanceManager.Domain.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<Budget?> GetByUserAndMonthAsync(Guid userId, int month, int year, CancellationToken cancellationToken = default);
    Task<IEnumerable<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Budget?> GetWithItemsAsync(Guid budgetId, CancellationToken cancellationToken = default);
    Task UpdateBudgetSpendingAsync(Guid userId, Guid categoryId, decimal amount, int month, int year, CancellationToken cancellationToken = default);
}
