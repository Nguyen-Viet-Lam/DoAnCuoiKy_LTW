using DoAnLTW.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<WalletTransaction> Transactions => Set<WalletTransaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<UserPersonalKeyword> UserPersonalKeywords => Set<UserPersonalKeyword>();
    public DbSet<BudgetAlert> BudgetAlerts => Set<BudgetAlert>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<ReportDispatchLog> ReportDispatchLogs => Set<ReportDispatchLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Username)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<EmailOtp>()
            .HasIndex(x => new { x.Email, x.Purpose, x.IsUsed });

        modelBuilder.Entity<Budget>()
            .HasIndex(x => new { x.UserId, x.CategoryId, x.Year, x.Month })
            .IsUnique();

        modelBuilder.Entity<UserPersonalKeyword>()
            .HasIndex(x => new { x.UserId, x.Keyword })
            .IsUnique();

        modelBuilder.Entity<ReportDispatchLog>()
            .HasIndex(x => new { x.UserId, x.ReportType, x.PeriodKey })
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(x => new { x.Name, x.TransactionType, x.IsDefault });

        modelBuilder.Entity<AppUser>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmailOtp>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Wallet>()
            .HasOne(x => x.User)
            .WithMany(x => x.Wallets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(x => x.User)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(x => x.Wallet)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Budget>()
            .HasOne(x => x.User)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Budget>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Budgets)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserPersonalKeyword>()
            .HasOne(x => x.User)
            .WithMany(x => x.PersonalKeywords)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserPersonalKeyword>()
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BudgetAlert>()
            .HasOne(x => x.User)
            .WithMany(x => x.Alerts)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BudgetAlert>()
            .HasOne(x => x.Budget)
            .WithMany(x => x.Alerts)
            .HasForeignKey(x => x.BudgetId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BudgetAlert>()
            .HasOne(x => x.WalletTransaction)
            .WithMany()
            .HasForeignKey(x => x.WalletTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SystemLog>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ReportDispatchLog>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
