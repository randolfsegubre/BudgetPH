using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FinanceManager.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<ApplicationUser> AppUsers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<CreditCardDetails> CreditCardDetails { get; set; }
    public DbSet<LoanDetails> LoanDetails { get; set; }
    public DbSet<LoanPaymentSchedule> LoanPaymentSchedules { get; set; }
    public DbSet<InvestmentAccountDetails> InvestmentAccountDetails { get; set; }
    public DbSet<InvestmentHolding> InvestmentHoldings { get; set; }
    public DbSet<InvestmentTransaction> InvestmentTransactions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetItem> BudgetItems { get; set; }
    public DbSet<SavingsGoal> SavingsGoals { get; set; }
    public DbSet<SavingsContribution> SavingsContributions { get; set; }
    public DbSet<IncomeSource> IncomeSources { get; set; }
    public DbSet<FinancialDocument> FinancialDocuments { get; set; }
    public DbSet<NetWorthSnapshot> NetWorthSnapshots { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Rename Identity tables with fm_ prefix
        builder.Entity<IdentityUser>().ToTable("fm_AspNetUsers");
        builder.Entity<IdentityRole>().ToTable("fm_AspNetRoles");
        builder.Entity<IdentityUserRole<string>>().ToTable("fm_AspNetUserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("fm_AspNetUserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("fm_AspNetUserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("fm_AspNetRoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("fm_AspNetUserTokens");

        // Global query filter for soft deletes
        builder.Entity<ApplicationUser>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Account>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Transaction>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Budget>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<SavingsGoal>().HasQueryFilter(e => !e.IsDeleted);

        // SQL Server does not allow multiple cascade paths; set restrict for entities not explicitly configured
        builder.Entity<RecurringTransaction>()
            .HasOne(r => r.User).WithMany(u => u.RecurringTransactions).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<IncomeSource>()
            .HasOne(i => i.User).WithMany(u => u.IncomeSources).HasForeignKey(i => i.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<FinancialDocument>()
            .HasOne(d => d.User).WithMany(u => u.Documents).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<NetWorthSnapshot>()
            .HasOne(n => n.User).WithMany(u => u.NetWorthSnapshots).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<UserNotification>()
            .HasOne(n => n.User).WithMany(u => u.Notifications).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<SavingsContribution>()
            .HasOne(sc => sc.SavingsGoal).WithMany(g => g.Contributions).HasForeignKey(sc => sc.SavingsGoalId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<InvestmentTransaction>()
            .HasOne(it => it.Holding).WithMany().HasForeignKey(it => it.HoldingId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<LoanPaymentSchedule>()
            .HasOne(lp => lp.LoanDetails).WithMany(l => l.PaymentSchedule).HasForeignKey(lp => lp.LoanDetailsId).OnDelete(DeleteBehavior.Restrict);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
