using FinanceManager.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace FinanceManager.Infrastructure.Identity;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }

    public virtual IdentityUser IdentityUser { get; set; } = null!;
}
