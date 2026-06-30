using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Controllers;

[Authorize]
[Route("api/savings-goals")]
public class SavingsGoalsController(ApplicationDbContext context) : BaseApiController
{
    // ─── DTOs ────────────────────────────────────────────────────────────────
    public record CreateGoalDto(
        string Name,
        string? Description,
        string? Icon,
        string? Color,
        decimal TargetAmount,
        DateOnly? TargetDate,
        int Priority = 1
    );

    public record ContributeDto(
        decimal Amount,
        bool IsWithdrawal,
        DateTime ContributionDate,
        string? Notes
    );

    public record SavingsGoalDto(
        Guid Id,
        string Name,
        string? Description,
        string? Icon,
        string? Color,
        decimal TargetAmount,
        decimal CurrentAmount,
        decimal ProgressPercentage,
        decimal RemainingAmount,
        DateOnly? TargetDate,
        int? DaysRemaining,
        bool IsCompleted,
        bool IsPaused,
        int Priority,
        DateTime CreatedAt
    );

    public record ContributionDto(
        Guid Id,
        decimal Amount,
        bool IsWithdrawal,
        DateTime ContributionDate,
        string? Notes
    );

    // ─── Endpoints ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var goals = await context.SavingsGoals
            .Where(g => g.UserId == appUser.Id)
            .OrderBy(g => g.IsCompleted).ThenBy(g => g.Priority).ThenBy(g => g.Name)
            .ToListAsync(ct);

        return Ok(goals.Select(MapGoal));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var goal = await context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == appUser.Id, ct);

        if (goal == null) return NotFound();
        return Ok(MapGoal(goal));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGoalDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        if (dto.TargetAmount <= 0)
            return BadRequest(new { error = "Target amount must be greater than zero." });

        var goal = new SavingsGoal
        {
            UserId = appUser.Id,
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Color = dto.Color,
            TargetAmount = dto.TargetAmount,
            TargetDate = dto.TargetDate,
            Priority = dto.Priority,
            CurrentAmount = 0,
        };

        context.SavingsGoals.Add(goal);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, MapGoal(goal));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateGoalDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var goal = await context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == appUser.Id, ct);
        if (goal == null) return NotFound();

        goal.Name = dto.Name;
        goal.Description = dto.Description;
        goal.Icon = dto.Icon;
        goal.Color = dto.Color;
        goal.TargetAmount = dto.TargetAmount;
        goal.TargetDate = dto.TargetDate;
        goal.Priority = dto.Priority;

        await context.SaveChangesAsync(ct);
        return Ok(MapGoal(goal));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var goal = await context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == appUser.Id, ct);
        if (goal == null) return NotFound();

        goal.IsDeleted = true;
        goal.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    // ─── Contributions ───────────────────────────────────────────────────────

    [HttpGet("{id:guid}/contributions")]
    public async Task<IActionResult> GetContributions(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var goal = await context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == appUser.Id, ct);
        if (goal == null) return NotFound();

        var contributions = await context.SavingsContributions
            .Where(c => c.SavingsGoalId == id)
            .OrderByDescending(c => c.ContributionDate)
            .ToListAsync(ct);

        return Ok(contributions.Select(c => new ContributionDto(
            c.Id, c.Amount, c.IsWithdrawal, c.ContributionDate, c.Notes)));
    }

    [HttpPost("{id:guid}/contributions")]
    public async Task<IActionResult> AddContribution(Guid id, [FromBody] ContributeDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var goal = await context.SavingsGoals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == appUser.Id, ct);
        if (goal == null) return NotFound();

        if (dto.Amount <= 0)
            return BadRequest(new { error = "Amount must be greater than zero." });

        var contribution = new SavingsContribution
        {
            SavingsGoalId = id,
            Amount = dto.Amount,
            IsWithdrawal = dto.IsWithdrawal,
            ContributionDate = dto.ContributionDate == default ? DateTime.UtcNow : dto.ContributionDate,
            Notes = dto.Notes,
        };

        // Update goal balance
        if (dto.IsWithdrawal)
            goal.CurrentAmount = Math.Max(goal.CurrentAmount - dto.Amount, 0);
        else
            goal.CurrentAmount += dto.Amount;

        // Mark complete if target reached
        if (!goal.IsCompleted && goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.IsCompleted = true;
            goal.CompletedAt = DateTime.UtcNow;
        }

        context.SavingsContributions.Add(contribution);
        await context.SaveChangesAsync(ct);

        return Ok(new ContributionDto(
            contribution.Id, contribution.Amount, contribution.IsWithdrawal,
            contribution.ContributionDate, contribution.Notes));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static SavingsGoalDto MapGoal(SavingsGoal g) => new(
        g.Id, g.Name, g.Description, g.Icon, g.Color,
        g.TargetAmount, g.CurrentAmount,
        g.ProgressPercentage, g.RemainingAmount,
        g.TargetDate, g.DaysRemaining,
        g.IsCompleted, g.IsPaused, g.Priority,
        g.CreatedAt
    );

    private async Task<ApplicationUser?> GetAppUserAsync(CancellationToken ct)
        => await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
}
