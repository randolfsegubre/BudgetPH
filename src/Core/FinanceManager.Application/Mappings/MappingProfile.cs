using AutoMapper;
using FinanceManager.Application.Features.Accounts.DTOs;
using FinanceManager.Application.Features.Budgets.DTOs;
using FinanceManager.Application.Features.Dashboard.DTOs;
using FinanceManager.Application.Features.Transactions.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Accounts
        // AccountDto is a record (constructor-mapped, no parameterless ctor). Its CreditCard/Loan/Investment
        // ctor params don't line up by name with the source's CreditCardDetails/LoanDetails/InvestmentAccountDetails
        // navigation properties, so AutoMapper can't bind them by convention. ForMember silently fails to redirect
        // to a ctor parameter for immutable/record types (it only rebinds settable members) — that caused AutoMapper
        // to fall back to Activator.CreateInstance(AccountDto), which throws because records have no 0-arg ctor.
        // ForCtorParam is the documented way to map into a specific constructor parameter, so use that instead.
        CreateMap<Account, AccountDto>()
            .ForCtorParam(nameof(AccountDto.CreditCard), o => o.MapFrom(s => s.CreditCardDetails))
            .ForCtorParam(nameof(AccountDto.Loan), o => o.MapFrom(s => s.LoanDetails))
            .ForCtorParam(nameof(AccountDto.Investment), o => o.MapFrom(s => s.InvestmentAccountDetails));

        CreateMap<CreditCardDetails, CreditCardSummaryDto>()
            .ForMember(d => d.AvailableCredit, o => o.MapFrom(s => s.CreditLimit + s.Account.Balance));

        CreateMap<LoanDetails, LoanSummaryDto>();
        CreateMap<InvestmentHolding, HoldingDto>();
        CreateMap<InvestmentAccountDetails, InvestmentSummaryDto>(
        ).ForMember(d => d.Holdings, o => o.MapFrom(s => s.Holdings));

        // Transactions
        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.AccountName, o => o.MapFrom(s => s.Account.Name))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.CategoryIcon, o => o.MapFrom(s => s.Category != null ? s.Category.Icon : null))
            .ForMember(d => d.CategoryColor, o => o.MapFrom(s => s.Category != null ? s.Category.Color : null));

        CreateMap<Transaction, RecentTransactionDto>()
            .ForMember(d => d.AccountName, o => o.MapFrom(s => s.Account.Name))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.CategoryIcon, o => o.MapFrom(s => s.Category != null ? s.Category.Icon : null))
            .ForMember(d => d.CategoryColor, o => o.MapFrom(s => s.Category != null ? s.Category.Color : null))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.TransactionType));

        // Budgets
        CreateMap<Budget, BudgetDto>()
            .ForMember(d => d.TotalAllocated, o => o.MapFrom(s => s.Items.Sum(i => i.AllocatedAmount)))
            .ForMember(d => d.TotalSpent, o => o.MapFrom(s => s.Items.Sum(i => i.SpentAmount)))
            .ForMember(d => d.RemainingBudget, o => o.MapFrom(s => s.TotalExpenseLimit - s.Items.Sum(i => i.SpentAmount)))
            .ForMember(d => d.OverallProgress, o => o.MapFrom(s =>
                s.TotalExpenseLimit > 0 ? (s.Items.Sum(i => i.SpentAmount) / s.TotalExpenseLimit) * 100 : 0));

        CreateMap<BudgetItem, BudgetItemDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryIcon, o => o.MapFrom(s => s.Category.Icon))
            .ForMember(d => d.CategoryColor, o => o.MapFrom(s => s.Category.Color));

        // Dashboard
        CreateMap<Account, AccountBalanceSummaryDto>();

        CreateMap<SavingsGoal, SavingsGoalProgressDto>()
            .ForMember(d => d.GoalId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Percentage, o => o.MapFrom(s => s.ProgressPercentage));
    }
}
