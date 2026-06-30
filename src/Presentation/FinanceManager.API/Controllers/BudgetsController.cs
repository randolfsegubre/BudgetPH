using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using FinanceManager.Application.Features.Budgets.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace FinanceManager.API.Controllers;

[Authorize]
[Route("api/budgets")]
public class BudgetsController(ApplicationDbContext context, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var budgets = await context.Budgets
            .Include(b => b.Items).ThenInclude(i => i.Category)
            .Where(b => b.UserId == appUser.Id)
            .OrderByDescending(b => b.Year).ThenByDescending(b => b.Month)
            .ToListAsync(ct);

        return Ok(mapper.Map<List<BudgetDto>>(budgets));
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var now = DateTime.UtcNow;
        var budget = await context.Budgets
            .Include(b => b.Items).ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(b => b.UserId == appUser.Id && b.Month == now.Month && b.Year == now.Year, ct);

        if (budget == null) return NotFound(new { message = "No budget set for current month." });
        return Ok(mapper.Map<BudgetDto>(budget));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBudgetDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        if (await context.Budgets.AnyAsync(b => b.UserId == appUser.Id && b.Month == dto.Month && b.Year == dto.Year && !b.IsTemplate, ct))
            return BadRequest(new { error = $"Budget for {dto.Month}/{dto.Year} already exists." });

        var budget = new Budget
        {
            UserId = appUser.Id,
            Name = dto.Name,
            Month = dto.Month,
            Year = dto.Year,
            TotalIncomeGoal = dto.TotalIncomeGoal,
            TotalExpenseLimit = dto.TotalExpenseLimit,
            Items = dto.Items.Select(i => new BudgetItem { CategoryId = i.CategoryId, AllocatedAmount = i.AllocatedAmount }).ToList()
        };

        context.Budgets.Add(budget);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetCurrent), mapper.Map<BudgetDto>(budget));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var budget = await context.Budgets.FirstOrDefaultAsync(b => b.Id == id && b.UserId == appUser.Id, ct);
        if (budget == null) return NotFound();

        budget.IsDeleted = true;
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    private async Task<ApplicationUser?> GetAppUserAsync(CancellationToken ct)
        => await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
}
