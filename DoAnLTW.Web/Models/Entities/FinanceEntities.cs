using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Web.Models.Entities;

public class Wallet
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(32)]
    public string Type { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal InitialBalance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; }

    [MaxLength(250)]
    public string? Note { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(16)]
    public string TransactionType { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Icon { get; set; } = "bi-circle";

    [MaxLength(16)]
    public string ColorHex { get; set; } = "#7fc8f8";

    public bool IsDefault { get; set; } = true;

    public int? CreatedByUserId { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public AppUser? CreatedByUser { get; set; }

    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}

public class WalletTransaction
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int WalletId { get; set; }

    public int CategoryId { get; set; }

    [Required, MaxLength(16)]
    public string Type { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(300)]
    public string Note { get; set; } = string.Empty;

    public DateTime OccurredOn { get; set; } = DateTime.Now;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? AiSuggestedCategoryId { get; set; }

    [MaxLength(100)]
    public string? AiSuggestedLabel { get; set; }

    public double AiConfidence { get; set; }

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    [ForeignKey(nameof(WalletId))]
    public Wallet Wallet { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}

public class Budget
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LimitAmount { get; set; }

    public int AlertThresholdPercent { get; set; } = 80;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    public ICollection<BudgetAlert> Alerts { get; set; } = new List<BudgetAlert>();
}

public class UserPersonalKeyword
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Keyword { get; set; } = string.Empty;

    public int HitCount { get; set; } = 1;

    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}

public class BudgetAlert
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BudgetId { get; set; }

    public int WalletTransactionId { get; set; }

    [MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SpentAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LimitAmount { get; set; }

    public double UsagePercent { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    [ForeignKey(nameof(BudgetId))]
    public Budget Budget { get; set; } = null!;

    [ForeignKey(nameof(WalletTransactionId))]
    public WalletTransaction WalletTransaction { get; set; } = null!;
}
