using FinanceManager.Infrastructure.Data;
using FinanceManager.Application.Features.Dashboard.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceManager.Domain.Enums;

namespace FinanceManager.API.Controllers;

[Authorize]
[Route("api/dashboard")]
public class DashboardController(ApplicationDbContext context) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
        if (appUser == null) return Unauthorized();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        // Accounts
        var accounts = await context.Accounts
            .Where(a => a.UserId == appUser.Id && a.IsActive)
            .ToListAsync(ct);

        var totalAssets = accounts.Where(a => a.Balance >= 0 && a.IncludeInNetWorth).Sum(a => a.Balance);
        var totalLiabilities = accounts.Where(a => a.Balance < 0 && a.IncludeInNetWorth).Sum(a => Math.Abs(a.Balance));

        // Monthly transactions
        var monthTransactions = await context.Transactions
            .Include(t => t.Account).Include(t => t.Category)
            .Where(t => t.UserId == appUser.Id && t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd)
            .ToListAsync(ct);

        var monthlyIncome = monthTransactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount);
        var monthlyExpenses = monthTransactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);
        var savingsRate = monthlyIncome > 0 ? ((monthlyIncome - monthlyExpenses) / monthlyIncome) * 100 : 0;

        // Recent transactions
        var recent = monthTransactions.OrderByDescending(t => t.TransactionDate).Take(10)
            .Select(t => new RecentTransactionDto(t.Id, t.Account.Name, t.Description,
                t.Category?.Name, t.Category?.Icon, t.Category?.Color, t.Amount, t.TransactionType, t.TransactionDate))
            .ToList();

        // Budget progress
        var budget = await context.Budgets
            .Include(b => b.Items).ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(b => b.UserId == appUser.Id && b.Month == now.Month && b.Year == now.Year, ct);

        var budgetProgress = budget?.Items.Select(i => new BudgetProgressDto(
            budget.Id, i.Category.Name, i.Category.Icon, i.Category.Color,
            i.AllocatedAmount, i.SpentAmount, i.PercentageUsed, i.IsOverBudget)).ToList() ?? [];

        // Savings goals
        var savingsGoals = await context.SavingsGoals
            .Where(s => s.UserId == appUser.Id && !s.IsCompleted)
            .OrderBy(s => s.Priority)
            .Take(5)
            .Select(s => new SavingsGoalProgressDto(s.Id, s.Name, s.Icon, s.Color, s.TargetAmount, s.CurrentAmount, s.ProgressPercentage, s.TargetDate, s.DaysRemaining))
            .ToListAsync(ct);

        // Top spending categories
        var topCategories = monthTransactions
            .Where(t => t.TransactionType == TransactionType.Expense && t.Category != null)
            .GroupBy(t => new { t.Category!.Name, t.Category.Icon, t.Category.Color })
            .Select(g => new CategoryBreakdownDto(g.Key.Name, g.Key.Icon, g.Key.Color, g.Sum(t => t.Amount), monthlyExpenses > 0 ? g.Sum(t => t.Amount) / monthlyExpenses * 100 : 0))
            .OrderByDescending(c => c.Amount).Take(5).ToList();

        // 6-month spending trend
        var trend6m = await context.Transactions
            .Where(t => t.UserId == appUser.Id && t.TransactionDate >= now.AddMonths(-6))
            .ToListAsync(ct);

        var spendingTrend = trend6m
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new SpendingByMonthDto(
                $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                g.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount),
                g.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount)))
            .OrderBy(s => s.Month).ToList();

        var accountSummaries = accounts.Select(a => new AccountBalanceSummaryDto(
            a.Id, a.Name, a.AccountType, a.Balance, a.Currency, a.InstitutionName, a.Color, a.Icon)).ToList();

        return Ok(new DashboardSummaryDto(
            totalAssets - totalLiabilities, totalAssets, totalLiabilities,
            monthlyIncome, monthlyExpenses, savingsRate, monthlyIncome - monthlyExpenses,
            accountSummaries, recent, budgetProgress, savingsGoals,
            [], new NetWorthTrendDto([], []), spendingTrend, topCategories));
    }
}
