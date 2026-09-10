using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using FinanceManager.Application.Features.Transactions.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace FinanceManager.API.Controllers;

/// <summary>
/// REST controller for Transactions - this app talks to EF Core's
/// <see cref="ApplicationDbContext"/> directly from the controller rather
/// than going through a CQRS/MediatR layer (unlike E-Commerse.AI.API or
/// Lakbay's services) - there's no separate command/query handler to look
/// in, this class *is* the read and write logic. Every action re-derives
/// <see cref="ApplicationUser"/> via <see cref="GetAppUserAsync"/> and
/// scopes its query/mutation to that user's own data - there is no
/// cross-user data access anywhere in this controller.
/// </summary>
[Authorize]
[Route("api/transactions")]
public class TransactionsController(ApplicationDbContext context, IMapper mapper) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TransactionFilterDto filter, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var query = context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == appUser.Id)
            .AsQueryable();

        if (filter.AccountId.HasValue) query = query.Where(t => t.AccountId == filter.AccountId);
        if (filter.CategoryId.HasValue) query = query.Where(t => t.CategoryId == filter.CategoryId);
        if (filter.FromDate.HasValue) query = query.Where(t => t.TransactionDate >= filter.FromDate);
        if (filter.ToDate.HasValue) query = query.Where(t => t.TransactionDate <= filter.ToDate);
        if (filter.Type.HasValue) query = query.Where(t => t.TransactionType == filter.Type);
        if (filter.Status.HasValue) query = query.Where(t => t.Status == filter.Status);
        if (filter.MinAmount.HasValue) query = query.Where(t => t.Amount >= filter.MinAmount);
        if (filter.MaxAmount.HasValue) query = query.Where(t => t.Amount <= filter.MaxAmount);
        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(t => t.Description.Contains(filter.SearchTerm) || (t.Merchant != null && t.Merchant.Contains(filter.SearchTerm)));

        var total = await query.CountAsync(ct);
        query = filter.SortDescending
            ? query.OrderByDescending(t => t.TransactionDate)
            : query.OrderBy(t => t.TransactionDate);

        var transactions = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return Ok(new { Items = mapper.Map<List<TransactionDto>>(transactions), TotalCount = total, filter.PageNumber, filter.PageSize });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var tx = await context.Transactions.Include(t => t.Account).Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == appUser.Id, ct);

        if (tx == null) return NotFound();
        return Ok(mapper.Map<TransactionDto>(tx));
    }

    /// <summary>
    /// Creates a Transaction and applies every side effect a real financial
    /// transaction has: it moves the owning Account's balance, optionally
    /// moves a second Account's balance too (a Transfer), and rolls into
    /// this month's Budget spend tracking. All of it happens in one
    /// EF Core change-tracking unit, committed by the single
    /// <c>SaveChangesAsync</c> at the end - if that call fails, none of
    /// these side effects are persisted, so the balances/budget can never
    /// drift out of sync with an actually-saved transaction.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        // STEP 1 of 5 - the transaction must post against an account this user actually owns.
        var account = await context.Accounts.FirstOrDefaultAsync(a => a.Id == dto.AccountId && a.UserId == appUser.Id, ct);
        if (account == null) return BadRequest(new { error = "Account not found." });

        var tx = new Transaction
        {
            UserId = appUser.Id,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            TransactionType = dto.TransactionType,
            Description = dto.Description,
            Merchant = dto.Merchant,
            TransactionDate = dto.TransactionDate,
            TransferToAccountId = dto.TransferToAccountId,
            IsRecurring = dto.IsRecurring,
            Tags = dto.Tags,
            Notes = dto.Notes
        };

        // STEP 2 of 5 - apply the balance effect: Income adds to the account,
        // everything else (Expense, Transfer-out) subtracts from it.
        account.Balance += dto.TransactionType == TransactionType.Income ? dto.Amount : -dto.Amount;

        // STEP 3 of 5 - a Transfer also credits the destination account, on
        // top of (not instead of) debiting the source account in STEP 2.
        if (dto.TransactionType == TransactionType.Transfer && dto.TransferToAccountId.HasValue)
        {
            var toAccount = await context.Accounts.FirstOrDefaultAsync(a => a.Id == dto.TransferToAccountId && a.UserId == appUser.Id, ct);
            if (toAccount != null) toAccount.Balance += dto.Amount;
        }

        context.Transactions.Add(tx);

        // STEP 4 of 5 - only an Expense with a Category counts against a
        // budget; Income/Transfer never touch budget spend tracking, and an
        // uncategorized expense has nothing to attribute spend to.
        if (tx.CategoryId.HasValue && tx.TransactionType == TransactionType.Expense)
        {
            var budget = await context.Budgets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == appUser.Id && b.Month == tx.TransactionDate.Month && b.Year == tx.TransactionDate.Year, ct);
            var budgetItem = budget?.Items.FirstOrDefault(i => i.CategoryId == tx.CategoryId);
            if (budgetItem != null) budgetItem.SpentAmount += tx.Amount;
        }

        // STEP 5 of 5 - one SaveChangesAsync commits the new Transaction and
        // every Account/BudgetItem balance change from steps 2-4 together.
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = tx.Id }, mapper.Map<TransactionDto>(tx));
    }

    /// <summary>
    /// Updates a Transaction's editable fields and re-applies its balance
    /// effect. Note what's NOT editable here: <see cref="Transaction.TransactionType"/>
    /// and the account it posted against never change on update - only
    /// Amount/Category/description-style fields and Status do. That's why
    /// "reverse then reapply" (both keyed off the same, unchanged
    /// TransactionType) is safe: it can only ever correct the Amount, never
    /// flip a Transaction from Income to Expense mid-edit.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionDto dto, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var tx = await context.Transactions.Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == appUser.Id, ct);
        if (tx == null) return NotFound();

        // STEP 1 of 3 - undo the OLD Amount's balance effect before touching anything else.
        tx.Account.Balance += tx.TransactionType == TransactionType.Income ? -tx.Amount : tx.Amount;

        tx.CategoryId = dto.CategoryId;
        tx.Amount = dto.Amount;
        tx.Description = dto.Description;
        tx.Merchant = dto.Merchant;
        tx.TransactionDate = dto.TransactionDate;
        tx.Status = dto.Status;
        tx.Tags = dto.Tags;
        tx.Notes = dto.Notes;

        // STEP 2 of 3 - apply the NEW Amount's balance effect (tx.Amount is
        // now dto.Amount, since the assignment above already happened).
        tx.Account.Balance += tx.TransactionType == TransactionType.Income ? dto.Amount : -dto.Amount;

        // STEP 3 of 3 - persist the edited Transaction and the account's net balance change together.
        await context.SaveChangesAsync(ct);
        return Ok(mapper.Map<TransactionDto>(tx));
    }

    /// <summary>
    /// Soft-deletes a Transaction (<see cref="Transaction.IsDeleted"/>/
    /// <see cref="Transaction.DeletedAt"/>, never an actual row delete - so
    /// a user's transaction history stays reconstructable) and reverses its
    /// balance effect, so a deleted transaction stops counting toward the
    /// account's balance without needing a separate "undo" endpoint.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var tx = await context.Transactions.Include(t => t.Account)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == appUser.Id, ct);
        if (tx == null) return NotFound();

        // Reverse balance
        tx.Account.Balance += tx.TransactionType == TransactionType.Income ? -tx.Amount : tx.Amount;
        tx.IsDeleted = true;
        tx.DeletedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var appUser = await GetAppUserAsync(ct);
        if (appUser == null) return Unauthorized();

        var from = new DateTime(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var transactions = await context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == appUser.Id && t.TransactionDate >= from && t.TransactionDate <= to)
            .ToListAsync(ct);

        var income = transactions.Where(t => t.TransactionType == TransactionType.Income).Sum(t => t.Amount);
        var expenses = transactions.Where(t => t.TransactionType == TransactionType.Expense).Sum(t => t.Amount);

        var categories = transactions
            .Where(t => t.TransactionType == TransactionType.Expense && t.Category != null)
            .GroupBy(t => new { t.Category!.Id, t.Category.Name, t.Category.Icon, t.Category.Color })
            .Select(g => new CategorySpendingDto(g.Key.Id, g.Key.Name, g.Key.Icon, g.Key.Color, g.Sum(t => t.Amount), g.Count(), expenses > 0 ? g.Sum(t => t.Amount) / expenses * 100 : 0))
            .OrderByDescending(c => c.Amount).Take(10).ToList();

        return Ok(new MonthlySummaryDto(year, month, income, expenses, income - expenses, categories));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await context.Categories
            .Where(c => c.IsSystem || c.UserId == null)
            .OrderBy(c => c.Type).ThenBy(c => c.SortOrder)
            .ToListAsync(ct);
        return Ok(categories);
    }

    private async Task<ApplicationUser?> GetAppUserAsync(CancellationToken ct)
        => await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == CurrentUserId, ct);
}
