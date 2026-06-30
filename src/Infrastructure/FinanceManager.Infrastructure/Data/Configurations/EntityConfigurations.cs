using FinanceManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("fm_Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(u => u.TimeZone).HasMaxLength(100).HasDefaultValue("Asia/Manila");
        builder.Property(u => u.PreferredCurrency).HasDefaultValue(Domain.Enums.Currency.PHP);
        builder.Property(u => u.IdentityUserId).HasMaxLength(450);
        builder.HasIndex(u => u.IdentityUserId).IsUnique().HasFilter("[IdentityUserId] IS NOT NULL");
    }
}

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("fm_Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.AccountNumber).HasMaxLength(50);
        builder.Property(a => a.Balance).HasPrecision(18, 4);
        builder.Property(a => a.OpeningBalance).HasPrecision(18, 4);
        builder.Property(a => a.InstitutionName).HasMaxLength(200);
        builder.Property(a => a.Color).HasMaxLength(20);
        builder.Property(a => a.Icon).HasMaxLength(100);
        builder.HasOne(a => a.User).WithMany(u => u.Accounts).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.CreditCardDetails).WithOne(c => c.Account).HasForeignKey<CreditCardDetails>(c => c.AccountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.LoanDetails).WithOne(l => l.Account).HasForeignKey<LoanDetails>(l => l.AccountId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.InvestmentAccountDetails).WithOne(i => i.Account).HasForeignKey<InvestmentAccountDetails>(i => i.AccountId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("fm_Transactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Amount).HasPrecision(18, 4);
        builder.Property(t => t.OriginalAmount).HasPrecision(18, 4);
        builder.Property(t => t.ExchangeRate).HasPrecision(18, 8);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(500);
        builder.Property(t => t.Merchant).HasMaxLength(200);
        builder.Property(t => t.Tags).HasMaxLength(500);
        builder.Property(t => t.Notes).HasMaxLength(1000);
        builder.HasIndex(t => new { t.UserId, t.TransactionDate });
        builder.HasIndex(t => new { t.AccountId, t.TransactionDate });
        builder.HasOne(t => t.Account).WithMany(a => a.Transactions).HasForeignKey(t => t.AccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Category).WithMany(c => c.Transactions).HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("fm_Categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Icon).HasMaxLength(100);
        builder.Property(c => c.Color).HasMaxLength(20);
        builder.HasOne(c => c.ParentCategory).WithMany(c => c.SubCategories).HasForeignKey(c => c.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("fm_Budgets");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.TotalIncomeGoal).HasPrecision(18, 4);
        builder.Property(b => b.TotalExpenseLimit).HasPrecision(18, 4);
        builder.HasIndex(b => new { b.UserId, b.Year, b.Month }).IsUnique().HasFilter("[IsTemplate] = 0");
        builder.Ignore(b => b.TotalAllocated);
        builder.Ignore(b => b.TotalSpent);
        builder.Ignore(b => b.RemainingBudget);
        builder.HasOne(b => b.User).WithMany(u => u.Budgets).HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BudgetItemConfiguration : IEntityTypeConfiguration<BudgetItem>
{
    public void Configure(EntityTypeBuilder<BudgetItem> builder)
    {
        builder.ToTable("fm_BudgetItems");
        builder.HasKey(bi => bi.Id);
        builder.Property(bi => bi.AllocatedAmount).HasPrecision(18, 4);
        builder.Property(bi => bi.SpentAmount).HasPrecision(18, 4);
        builder.Ignore(bi => bi.RemainingAmount);
        builder.Ignore(bi => bi.PercentageUsed);
        builder.Ignore(bi => bi.IsOverBudget);
        builder.HasOne(bi => bi.Budget).WithMany(b => b.Items).HasForeignKey(bi => bi.BudgetId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(bi => bi.Category).WithMany(c => c.BudgetItems).HasForeignKey(bi => bi.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SavingsGoalConfiguration : IEntityTypeConfiguration<SavingsGoal>
{
    public void Configure(EntityTypeBuilder<SavingsGoal> builder)
    {
        builder.ToTable("fm_SavingsGoals");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.TargetAmount).HasPrecision(18, 4);
        builder.Property(s => s.CurrentAmount).HasPrecision(18, 4);
        builder.Ignore(s => s.ProgressPercentage);
        builder.Ignore(s => s.RemainingAmount);
        builder.Ignore(s => s.DaysRemaining);
        builder.HasOne(s => s.User).WithMany(u => u.SavingsGoals).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class LoanDetailsConfiguration : IEntityTypeConfiguration<LoanDetails>
{
    public void Configure(EntityTypeBuilder<LoanDetails> builder)
    {
        builder.ToTable("fm_LoanDetails");
        builder.Property(l => l.OriginalAmount).HasPrecision(18, 4);
        builder.Property(l => l.OutstandingBalance).HasPrecision(18, 4);
        builder.Property(l => l.InterestRate).HasPrecision(8, 4);
        builder.Property(l => l.MonthlyPayment).HasPrecision(18, 4);
        builder.Property(l => l.LenderName).HasMaxLength(200);
        builder.Property(l => l.LoanNumber).HasMaxLength(100);
        builder.Ignore(l => l.TotalPayments);
        builder.Ignore(l => l.RemainingPayments);
    }
}

public class InvestmentHoldingConfiguration : IEntityTypeConfiguration<InvestmentHolding>
{
    public void Configure(EntityTypeBuilder<InvestmentHolding> builder)
    {
        builder.ToTable("fm_InvestmentHoldings");
        builder.Property(h => h.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Shares).HasPrecision(18, 8);
        builder.Property(h => h.AverageCostBasis).HasPrecision(18, 6);
        builder.Property(h => h.CurrentPrice).HasPrecision(18, 6);
        builder.Property(h => h.RealizedGainLoss).HasPrecision(18, 4);
        builder.Ignore(h => h.MarketValue);
        builder.Ignore(h => h.TotalCostBasis);
        builder.Ignore(h => h.UnrealizedGainLoss);
        builder.Ignore(h => h.UnrealizedGainLossPercent);
    }
}
