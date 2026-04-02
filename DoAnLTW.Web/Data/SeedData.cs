using DoAnLTW.Web.Data.Seeders;

namespace DoAnLTW.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, bool includeDemoData)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        await db.Database.EnsureCreatedAsync();

        await ReferenceDataSeeder.SeedAsync(scope.ServiceProvider);

        if (includeDemoData)
        {
            await DemoFinanceDataSeeder.SeedAsync(scope.ServiceProvider);
        }
    }
}
