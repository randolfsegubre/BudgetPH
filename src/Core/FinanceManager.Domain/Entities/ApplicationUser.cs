using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? TimeZone { get; set; } = "Asia/Manila";
    public Currency PreferredCurrency { get; set; } = Currency.PHP;
    public string? IdentityUserId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    // Navigation properties
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public virtual ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
    public virtual ICollection<IncomeSource> IncomeSources { get; set; } = new List<IncomeSource>();
    public virtual ICollection<FinancialDocument> Documents { get; set; } = new List<FinancialDocument>();
    public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();
    public virtual ICollection<NetWorthSnapshot> NetWorthSnapshots { get; set; } = new List<NetWorthSnapshot>();
    public virtual ICollection<UserNotification> Notifications { get; set; } = new List<UserNotification>();
}
