using FinanceManager.Domain.Common;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public CategoryType Type { get; set; }
    public bool IsSystem { get; set; } = false;
    public Guid? UserId { get; set; } // Null = system category
    public Guid? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }

    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
}
