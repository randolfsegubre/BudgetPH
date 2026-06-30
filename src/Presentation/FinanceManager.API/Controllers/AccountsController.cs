using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceManager.Application.Features.Accounts.DTOs;
using AutoMapper;

namespace FinanceManager.API.Controllers;

[Authorize]
[Route("api/accounts")]
public class AccountsController(ApplicationDbContext context, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var accounts = await context.Accounts
            .Include(a => a.CreditCardDetails)
            .Include(a => a.LoanDetails)
            .Include(a => a.InvestmentAccountDetails).ThenInclude(i => i.Holdings)
            .Where(a => a.UserId == appUser.Id && a.IsActive)
            .OrderBy(a => a.AccountType).ThenBy(a => a.Name)
            .ToListAsync(ct);

        return Ok(mapper.Map<List<AccountDto>>(accounts));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var account = await context.Accounts
            .Include(a => a.CreditCardDetails)
            .Include(a => a.LoanDetails)
            .Include(a => a.InvestmentAccountDetails)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == appUser.Id, ct);

        if (account == null) return NotFound();
        return Ok(mapper.Map<AccountDto>(account));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var account = new Account
        {
            UserId = appUser.Id,
            Name = dto.Name,
            AccountNumber = dto.AccountNumber,
            AccountType = dto.AccountType,
            Balance = dto.OpeningBalance,
            OpeningBalance = dto.OpeningBalance,
            Currency = dto.Currency,
            InstitutionName = dto.InstitutionName,
            OpeningDate = dto.OpeningDate,
            Color = dto.Color,
            Icon = dto.Icon,
            IncludeInNetWorth = dto.IncludeInNetWorth
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, mapper.Map<AccountDto>(account));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == appUser.Id, ct);
        if (account == null) return NotFound();

        account.Name = dto.Name;
        account.AccountNumber = dto.AccountNumber;
        account.InstitutionName = dto.InstitutionName;
        account.IsActive = dto.IsActive;
        account.Color = dto.Color;
        account.Icon = dto.Icon;
        account.IncludeInNetWorth = dto.IncludeInNetWorth;
        account.Notes = dto.Notes;

        await context.SaveChangesAsync(ct);
        return Ok(mapper.Map<AccountDto>(account));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == appUser.Id, ct);
        if (account == null) return NotFound();

        account.IsDeleted = true;
        account.DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var accounts = await context.Accounts
            .Where(a => a.UserId == appUser.Id && a.IsActive && a.IncludeInNetWorth)
            .ToListAsync(ct);

        var totalAssets = accounts.Where(a => a.Balance >= 0).Sum(a => a.Balance);
        var totalLiabilities = accounts.Where(a => a.Balance < 0).Sum(a => Math.Abs(a.Balance));

        return Ok(new { TotalAssets = totalAssets, TotalLiabilities = totalLiabilities, NetWorth = totalAssets - totalLiabilities });
    }

    private async Task<ApplicationUser?> GetAppUserAsync(CancellationToken ct)
        => await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
}
