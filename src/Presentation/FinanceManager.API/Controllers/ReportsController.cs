using FinanceManager.Application.Features.Reports.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Controllers;

[Authorize]
[Route("api/reports")]
public class ReportsController(IReportService reportService, ApplicationDbContext context) : BaseApiController
{
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] ReportRequestDto request, CancellationToken ct)
    {
        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
        if (appUser == null) return Unauthorized();

        var result = await reportService.GenerateReportAsync(appUser.Id, request, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });

        var report = result.Value!;
        return File(report.FileContent, report.ContentType, report.FileName);
    }

    [HttpGet("net-worth-trend")]
    public async Task<IActionResult> GetNetWorthTrend([FromQuery] int months = 12, CancellationToken ct = default)
    {
        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
        if (appUser == null) return Unauthorized();

        var snapshots = await context.NetWorthSnapshots
            .Where(n => n.UserId == appUser.Id)
            .OrderByDescending(n => n.SnapshotDate)
            .Take(months)
            .OrderBy(n => n.SnapshotDate)
            .ToListAsync(ct);

        return Ok(new
        {
            Labels = snapshots.Select(s => s.SnapshotDate.ToString("MMM yyyy")).ToList(),
            Values = snapshots.Select(s => s.NetWorth).ToList()
        });
    }

    [HttpGet("spending-trend")]
    public async Task<IActionResult> GetSpendingTrend([FromQuery] int months = 6, CancellationToken ct = default)
    {
        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
        if (appUser == null) return Unauthorized();

        var from = DateTime.UtcNow.AddMonths(-months);
        var transactions = await context.Transactions
            .Where(t => t.UserId == appUser.Id && t.TransactionDate >= from)
            .ToListAsync(ct);

        var trend = transactions
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new
            {
                Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                Income = g.Where(t => t.TransactionType == Domain.Enums.TransactionType.Income).Sum(t => t.Amount),
                Expenses = g.Where(t => t.TransactionType == Domain.Enums.TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(t => t.Month).ToList();

        return Ok(trend);
    }
}
