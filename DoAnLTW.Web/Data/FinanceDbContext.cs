using DoAnLTW.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Data;

/// <summary>DbContext trung tâm quản lý các bảng và quan hệ dữ liệu của hệ thống tài chính.</summary>
public class FinanceDbContext : DbContext
{
    /// <summary>
    /// Khởi tạo DbContext với cấu hình kết nối và mapping do DI cung cấp.
    /// </summary>
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Bảng vai trò dùng cho phân quyền người dùng trong hệ thống.
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();
    /// <summary>
    /// Bảng tài khoản người dùng và quản trị viên.
    /// </summary>
    public DbSet<AppUser> Users => Set<AppUser>();
    /// <summary>
    /// Bảng lưu OTP đã băm phục vụ đăng ký và đặt lại mật khẩu.
    /// </summary>
    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();
    /// <summary>
    /// Bảng ví hoặc nguồn tiền của người dùng.
    /// </summary>
    public DbSet<Wallet> Wallets => Set<Wallet>();
    /// <summary>
    /// Bảng danh mục chuẩn và danh mục do quản trị quản lý.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();
    /// <summary>
    /// Bảng giao dịch thu chi phát sinh trên các ví.
    /// </summary>
    public DbSet<WalletTransaction> Transactions => Set<WalletTransaction>();
    /// <summary>
    /// Bảng ngân sách theo danh mục và theo tháng.
    /// </summary>
    public DbSet<Budget> Budgets => Set<Budget>();
    /// <summary>
    /// Bảng từ khóa cá nhân dùng cho cơ chế học gợi ý danh mục.
    /// </summary>
    public DbSet<UserPersonalKeyword> UserPersonalKeywords => Set<UserPersonalKeyword>();
    /// <summary>
    /// Bảng lưu các cảnh báo vượt ngân sách.
    /// </summary>
    public DbSet<BudgetAlert> BudgetAlerts => Set<BudgetAlert>();
    /// <summary>
    /// Bảng nhật ký hệ thống phục vụ quản trị và truy vết.
    /// </summary>
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    /// <summary>
    /// Bảng lịch sử gửi báo cáo qua email.
    /// </summary>
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
