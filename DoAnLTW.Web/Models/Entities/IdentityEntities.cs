using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Web.Models.Entities;

public class Role
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(32)]
    public string Name { get; set; } = string.Empty;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}

public class AppUser
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? AvatarPath { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsEmailVerified { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;

    public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<UserPersonalKeyword> PersonalKeywords { get; set; } = new List<UserPersonalKeyword>();
    public ICollection<BudgetAlert> Alerts { get; set; } = new List<BudgetAlert>();
}

public class EmailOtp
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(32)]
    public string Purpose { get; set; } = string.Empty;

    [MaxLength(256)]
    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public int Attempts { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser? User { get; set; }
}
