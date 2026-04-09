using DoAnLTW.Web.Data.Seeders;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Data;

/// <summary>Lớp điều phối migrate database và chạy các seeder khi ứng dụng khởi động.</summary>
public static class SeedData
{
    /// <summary>
    /// Khởi tạo dữ liệu mẫu và dữ liệu tham chiếu khi ứng dụng bắt đầu.
    /// </summary>
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        bool includeDemoData,
        string? seedShowcaseUserEmail = null)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        await db.Database.MigrateAsync();

        await ReferenceDataSeeder.SeedAsync(scope.ServiceProvider);

        if (includeDemoData)
        {
            await DemoFinanceDataSeeder.SeedAsync(scope.ServiceProvider);
        }

        if (!string.IsNullOrWhiteSpace(seedShowcaseUserEmail))
        {
            await DemoFinanceDataSeeder.SeedShowcaseUserAsync(scope.ServiceProvider, seedShowcaseUserEmail);
        }
    }
}
