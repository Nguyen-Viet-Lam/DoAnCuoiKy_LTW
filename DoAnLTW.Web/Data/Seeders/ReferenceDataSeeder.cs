using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Models.Options;
using DoAnLTW.Web.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DoAnLTW.Web.Data.Seeders;

public static class ReferenceDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<FinanceDbContext>();
        var passwordService = serviceProvider.GetRequiredService<PasswordService>();
        var adminOptions = serviceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        var defaultCategories = GetDefaultCategories().ToList();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "User" });
            await db.SaveChangesAsync();
        }

        await SyncDefaultCategoriesAsync(db, defaultCategories);

        var adminRole = await db.Roles.FirstAsync(x => x.Name == "Admin");
        var normalizedEmail = adminOptions.Email.Trim().ToLowerInvariant();
        var normalizedUsername = adminOptions.Username.Trim();
        var displayName = adminOptions.DisplayName.Trim();
        var password = adminOptions.Password.Trim();

        if (string.IsNullOrWhiteSpace(password))
        {
            await db.SaveChangesAsync();
            return;
        }

        var adminUser = await db.Users.FirstOrDefaultAsync(x =>
            x.Email == normalizedEmail || x.Username == normalizedUsername);

        if (adminUser is null)
        {
            db.Users.Add(new AppUser
            {
                Username = normalizedUsername,
                DisplayName = displayName,
                Email = normalizedEmail,
                PasswordHash = passwordService.Hash(password),
                RoleId = adminRole.Id,
                IsActive = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            });
        }
        else
        {
            adminUser.Username = normalizedUsername;
            adminUser.DisplayName = displayName;
            adminUser.Email = normalizedEmail;
            adminUser.PasswordHash = passwordService.Hash(password);
            adminUser.RoleId = adminRole.Id;
            adminUser.IsActive = true;
            adminUser.IsEmailVerified = true;
        }

        await db.SaveChangesAsync();
    }

    private static IEnumerable<Category> GetDefaultCategories()
    {
        return
        [
            new Category { Name = "\u0102n u\u1ed1ng", TransactionType = "Expense", Icon = "bi-cup-hot", ColorHex = "#f7b267", IsDefault = true },
            new Category { Name = "Di chuy\u1ec3n", TransactionType = "Expense", Icon = "bi-bicycle", ColorHex = "#8ecae6", IsDefault = true },
            new Category { Name = "Nh\u00e0 \u1edf", TransactionType = "Expense", Icon = "bi-house-heart", ColorHex = "#b8c0ff", IsDefault = true },
            new Category { Name = "Mua s\u1eafm", TransactionType = "Expense", Icon = "bi-bag-heart", ColorHex = "#f7cad0", IsDefault = true },
            new Category { Name = "Gi\u1ea3i tr\u00ed", TransactionType = "Expense", Icon = "bi-controller", ColorHex = "#cdb4db", IsDefault = true },
            new Category { Name = "S\u1ee9c kh\u1ecfe", TransactionType = "Expense", Icon = "bi-heart-pulse", ColorHex = "#a9def9", IsDefault = true },
            new Category { Name = "H\u00f3a \u0111\u01a1n", TransactionType = "Expense", Icon = "bi-receipt", ColorHex = "#d0f4de", IsDefault = true },
            new Category { Name = "H\u1ecdc t\u1eadp", TransactionType = "Expense", Icon = "bi-journal-bookmark", ColorHex = "#bde0fe", IsDefault = true },
            new Category { Name = "Kh\u00e1c", TransactionType = "Expense", Icon = "bi-three-dots", ColorHex = "#e9ecef", IsDefault = true },
            new Category { Name = "L\u01b0\u01a1ng", TransactionType = "Income", Icon = "bi-cash-stack", ColorHex = "#95d5b2", IsDefault = true },
            new Category { Name = "Th\u01b0\u1edfng", TransactionType = "Income", Icon = "bi-trophy", ColorHex = "#ffd166", IsDefault = true },
            new Category { Name = "B\u00e1n h\u00e0ng", TransactionType = "Income", Icon = "bi-shop", ColorHex = "#90e0ef", IsDefault = true },
            new Category { Name = "Qu\u00e0 t\u1eb7ng", TransactionType = "Income", Icon = "bi-gift", ColorHex = "#ffc8dd", IsDefault = true },
            new Category { Name = "Thu kh\u00e1c", TransactionType = "Income", Icon = "bi-plus-circle", ColorHex = "#caf0f8", IsDefault = true }
        ];
    }

    private static async Task SyncDefaultCategoriesAsync(FinanceDbContext db, IReadOnlyCollection<Category> defaultCategories)
    {
        var existingCategories = await db.Categories.ToListAsync();

        foreach (var template in defaultCategories)
        {
            var existing = existingCategories.FirstOrDefault(x =>
                x.IsDefault &&
                x.TransactionType == template.TransactionType &&
                x.Icon == template.Icon);

            if (existing is null)
            {
                db.Categories.Add(new Category
                {
                    Name = template.Name,
                    TransactionType = template.TransactionType,
                    Icon = template.Icon,
                    ColorHex = template.ColorHex,
                    IsDefault = true
                });
                continue;
            }

            existing.Name = template.Name;
            existing.ColorHex = template.ColorHex;
            existing.IsDefault = true;
        }
    }
}
