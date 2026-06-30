using FinanceManager.Domain.Entities;

namespace FinanceManager.Domain.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Account?> GetWithDetailsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalAssetsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalLiabilitiesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
