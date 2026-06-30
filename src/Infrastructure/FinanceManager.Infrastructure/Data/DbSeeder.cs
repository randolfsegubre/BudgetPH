using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync(c => c.IsSystem))
            return;

        var categories = new List<Category>
        {
            // Income categories
            new() { Id = Guid.NewGuid(), Name = "Salary", Icon = "💼", Color = "#22c55e", Type = CategoryType.Income, IsSystem = true, SortOrder = 1 },
            new() { Id = Guid.NewGuid(), Name = "Freelance", Icon = "💻", Color = "#16a34a", Type = CategoryType.Income, IsSystem = true, SortOrder = 2 },
            new() { Id = Guid.NewGuid(), Name = "Business Income", Icon = "🏢", Color = "#15803d", Type = CategoryType.Income, IsSystem = true, SortOrder = 3 },
            new() { Id = Guid.NewGuid(), Name = "Investment Returns", Icon = "📈", Color = "#14532d", Type = CategoryType.Income, IsSystem = true, SortOrder = 4 },
            new() { Id = Guid.NewGuid(), Name = "Remittance Received", Icon = "💸", Color = "#4ade80", Type = CategoryType.Income, IsSystem = true, SortOrder = 5 },
            new() { Id = Guid.NewGuid(), Name = "Rental Income", Icon = "🏠", Color = "#86efac", Type = CategoryType.Income, IsSystem = true, SortOrder = 6 },
            new() { Id = Guid.NewGuid(), Name = "Other Income", Icon = "💰", Color = "#bbf7d0", Type = CategoryType.Income, IsSystem = true, SortOrder = 7 },

            // Expense categories — Philippine context
            new() { Id = Guid.NewGuid(), Name = "Food & Dining", Icon = "🍽️", Color = "#f97316", Type = CategoryType.Expense, IsSystem = true, SortOrder = 10 },
            new() { Id = Guid.NewGuid(), Name = "Groceries", Icon = "🛒", Color = "#ea580c", Type = CategoryType.Expense, IsSystem = true, SortOrder = 11 },
            new() { Id = Guid.NewGuid(), Name = "Transportation", Icon = "🚗", Color = "#3b82f6", Type = CategoryType.Expense, IsSystem = true, SortOrder = 12 },
            new() { Id = Guid.NewGuid(), Name = "Utilities", Icon = "💡", Color = "#eab308", Type = CategoryType.Expense, IsSystem = true, SortOrder = 13 },
            new() { Id = Guid.NewGuid(), Name = "Load & Internet", Icon = "📱", Color = "#06b6d4", Type = CategoryType.Expense, IsSystem = true, SortOrder = 14 },
            new() { Id = Guid.NewGuid(), Name = "Healthcare", Icon = "🏥", Color = "#ef4444", Type = CategoryType.Expense, IsSystem = true, SortOrder = 15 },
            new() { Id = Guid.NewGuid(), Name = "Education", Icon = "📚", Color = "#8b5cf6", Type = CategoryType.Expense, IsSystem = true, SortOrder = 16 },
            new() { Id = Guid.NewGuid(), Name = "Housing / Rent", Icon = "🏡", Color = "#a78bfa", Type = CategoryType.Expense, IsSystem = true, SortOrder = 17 },
            new() { Id = Guid.NewGuid(), Name = "Clothing & Apparel", Icon = "👗", Color = "#ec4899", Type = CategoryType.Expense, IsSystem = true, SortOrder = 18 },
            new() { Id = Guid.NewGuid(), Name = "Entertainment", Icon = "🎬", Color = "#f43f5e", Type = CategoryType.Expense, IsSystem = true, SortOrder = 19 },
            new() { Id = Guid.NewGuid(), Name = "Personal Care", Icon = "💄", Color = "#d946ef", Type = CategoryType.Expense, IsSystem = true, SortOrder = 20 },
            new() { Id = Guid.NewGuid(), Name = "Insurance", Icon = "🛡️", Color = "#0ea5e9", Type = CategoryType.Expense, IsSystem = true, SortOrder = 21 },
            new() { Id = Guid.NewGuid(), Name = "Loan Payments", Icon = "🏦", Color = "#64748b", Type = CategoryType.Expense, IsSystem = true, SortOrder = 22 },
            new() { Id = Guid.NewGuid(), Name = "Family Support / Allowance", Icon = "👨‍👩‍👧", Color = "#78716c", Type = CategoryType.Expense, IsSystem = true, SortOrder = 23 },
            new() { Id = Guid.NewGuid(), Name = "Savings / Investment", Icon = "🐷", Color = "#f59e0b", Type = CategoryType.Expense, IsSystem = true, SortOrder = 24 },
            new() { Id = Guid.NewGuid(), Name = "Subscriptions", Icon = "📺", Color = "#7c3aed", Type = CategoryType.Expense, IsSystem = true, SortOrder = 25 },
            new() { Id = Guid.NewGuid(), Name = "Taxes / Government Fees", Icon = "🏛️", Color = "#4b5563", Type = CategoryType.Expense, IsSystem = true, SortOrder = 26 },
            new() { Id = Guid.NewGuid(), Name = "Charitable Giving", Icon = "🙏", Color = "#fbbf24", Type = CategoryType.Expense, IsSystem = true, SortOrder = 27 },
            new() { Id = Guid.NewGuid(), Name = "Travel", Icon = "✈️", Color = "#06b6d4", Type = CategoryType.Expense, IsSystem = true, SortOrder = 28 },
            new() { Id = Guid.NewGuid(), Name = "Miscellaneous", Icon = "📦", Color = "#9ca3af", Type = CategoryType.Expense, IsSystem = true, SortOrder = 99 },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
