using System.Globalization;
using System.Text;
using DoAnLTW.Web.Models.Entities;
using DoAnLTW.Web.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace DoAnLTW.Web.Data.Seeders;

public static class DemoFinanceDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<FinanceDbContext>();
        var passwordService = serviceProvider.GetRequiredService<PasswordService>();
        var userRoleId = await db.Roles.Where(x => x.Name == "User").Select(x => x.Id).FirstAsync();

        var demoUsers = new[]
        {
            new DemoUserSeed(
                "userdemo",
                "demo@financeflow.local",
                "User123!",
                "Nguy\u1ec5n Minh An",
                true,
                true,
                DateTime.UtcNow.AddDays(-21)),
            new DemoUserSeed(
                "tranguser",
                "trang@financeflow.local",
                "User123!",
                "Tr\u1ea7n H\u00e0 Trang",
                false,
                true,
                DateTime.UtcNow.AddDays(-8)),
            new DemoUserSeed(
                "lamviet",
                "lam@financeflow.local",
                "User123!",
                "Nguy\u1ec5n Vi\u1ebft L\u00e3m",
                true,
                true,
                DateTime.UtcNow.AddDays(-16)),
            new DemoUserSeed(
                "ngocloi",
                "loi@financeflow.local",
                "User123!",
                "Tr\u1ea7n Ng\u1ecdc L\u1ee3i",
                true,
                true,
                DateTime.UtcNow.AddDays(-14)),
            new DemoUserSeed(
                "daiky",
                "ky@financeflow.local",
                "User123!",
                "Nguy\u1ec5n \u0110\u1ea1i K\u1ef3",
                true,
                true,
                DateTime.UtcNow.AddDays(-11)),
            new DemoUserSeed(
                "tranthuc",
                "thuc@financeflow.local",
                "User123!",
                "T\u00f4 Tr\u1ea7n Th\u1ee9c",
                true,
                true,
                DateTime.UtcNow.AddDays(-6)),
            new DemoUserSeed(
                "lamnguyenadc",
                "lamnguyenadc@gmail.com",
                "User123!",
                "Lam Nguyen ADC",
                true,
                true,
                DateTime.UtcNow.AddDays(-6))
        };

        foreach (var seed in demoUsers)
        {
            await EnsureUserAsync(db, passwordService, userRoleId, seed);
        }

        await db.SaveChangesAsync();

        var categories = await BuildCategoryLookupAsync(db);

        var primaryUser = await db.Users.FirstAsync(x => x.Email == "demo@financeflow.local");
        await EnsurePrimaryDemoProfileAsync(db, primaryUser.Id, categories);

        var lamUser = await db.Users.FirstAsync(x => x.Email == "lam@financeflow.local");
        await EnsureMaleDemoProfileAsync(
            db,
            lamUser.Id,
            categories,
            cashInitialBalance: 1_400_000m,
            bankInitialBalance: 4_800_000m,
            savingInitialBalance: 2_500_000m,
            bankWalletName: "MBBank",
            savingWalletName: "Qu\u1ef9 d\u1ef1 ph\u00f2ng",
            dayShift: 0,
            noteSet:
            [
                "\u0110\u1ed5 x\u0103ng \u0111i h\u1ecdc",
                "B\u00fan ch\u1ea3 H\u00e0 N\u1ed9i",
                "B\u00e1nh cu\u1ed1n bu\u1ed5i s\u00e1ng",
                "Mua s\u1eafm \u00e1o s\u01a1 mi",
                "Nh\u1eadn l\u01b0\u01a1ng part-time"
            ]);

        var loiUser = await db.Users.FirstAsync(x => x.Email == "loi@financeflow.local");
        await EnsureMaleDemoProfileAsync(
            db,
            loiUser.Id,
            categories,
            cashInitialBalance: 1_100_000m,
            bankInitialBalance: 5_300_000m,
            savingInitialBalance: 1_700_000m,
            bankWalletName: "BIDV",
            savingWalletName: "Qu\u1ef9 du l\u1ecbch",
            dayShift: 2,
            noteSet:
            [
                "C\u01a1m t\u1ea5m tr\u01b0a",
                "\u0110\u1ed5 x\u0103ng cu\u1ed1i tu\u1ea7n",
                "B\u00fan ch\u1ea3 t\u1ed1i",
                "Mua s\u1eafm gi\u00e0y th\u1ec3 thao",
                "Gia \u0111\u00ecnh chuy\u1ec3n th\u00eam"
            ]);

        var kyUser = await db.Users.FirstAsync(x => x.Email == "ky@financeflow.local");
        await EnsureMaleDemoProfileAsync(
            db,
            kyUser.Id,
            categories,
            cashInitialBalance: 1_250_000m,
            bankInitialBalance: 6_500_000m,
            savingInitialBalance: 2_900_000m,
            bankWalletName: "ACB",
            savingWalletName: "Qu\u1ef9 m\u1ee5c ti\u00eau",
            dayShift: 4,
            noteSet:
            [
                "B\u00e1nh cu\u1ed1n n\u00f3ng",
                "C\u00e0 ph\u00ea g\u1eb7p b\u1ea1n",
                "Mua s\u1eafm balo \u0111i h\u1ecdc",
                "\u0110\u1ed5 x\u0103ng \u0111i l\u00e0m",
                "Nh\u1eadn l\u01b0\u01a1ng freelance"
            ]);

        var thucUser = await db.Users.FirstAsync(x => x.Email == "thuc@financeflow.local");
        await EnsureMaleDemoProfileAsync(
            db,
            thucUser.Id,
            categories,
            cashInitialBalance: 950_000m,
            bankInitialBalance: 4_200_000m,
            savingInitialBalance: 2_100_000m,
            bankWalletName: "Techcombank",
            savingWalletName: "Qu\u1ef9 laptop",
            dayShift: 6,
            noteSet:
            [
                "C\u01a1m t\u1ea5m s\u01b0\u1eddn",
                "B\u00fan ch\u1ea3 g\u1ea7n tr\u01b0\u1eddng",
                "\u0110\u1ed5 x\u0103ng xe m\u00e1y",
                "Mua s\u1eafm ph\u1ee5 ki\u1ec7n c\u00f4ng ngh\u1ec7",
                "Th\u01b0\u1edfng d\u1ef1 \u00e1n nh\u1ecf"
            ]);

        var showcaseUser = await db.Users.FirstAsync(x => x.Email == "lamnguyenadc@gmail.com");
        await EnsurePrimaryDemoProfileAsync(db, showcaseUser.Id, categories);
        await EnsureShowcaseYearProfileAsync(db, showcaseUser.Id, categories);
        await SeedAlertsAsync(db, showcaseUser.Id, GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"));

        await SeedAlertsAsync(db, primaryUser.Id, GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"));
        await SeedLogsAsync(db, primaryUser.Id);
    }

    private static async Task EnsureUserAsync(
        FinanceDbContext db,
        PasswordService passwordService,
        int userRoleId,
        DemoUserSeed seed)
    {
        var normalizedEmail = seed.Email.Trim().ToLowerInvariant();
        var existingUser = await db.Users.FirstOrDefaultAsync(x =>
            x.Email == normalizedEmail || x.Username == seed.Username);

        if (existingUser is null)
        {
            db.Users.Add(new AppUser
            {
                Username = seed.Username,
                Email = normalizedEmail,
                DisplayName = seed.DisplayName,
                PasswordHash = passwordService.Hash(seed.Password),
                RoleId = userRoleId,
                IsActive = seed.IsActive,
                IsEmailVerified = seed.IsEmailVerified,
                CreatedAt = seed.CreatedAt
            });
            return;
        }

        existingUser.Username = seed.Username;
        existingUser.Email = normalizedEmail;
        existingUser.DisplayName = seed.DisplayName;
        existingUser.PasswordHash = passwordService.Hash(seed.Password);
        existingUser.RoleId = userRoleId;
        existingUser.IsActive = seed.IsActive;
        existingUser.IsEmailVerified = seed.IsEmailVerified;
    }

    private static async Task EnsurePrimaryDemoProfileAsync(
        FinanceDbContext db,
        int userId,
        IReadOnlyDictionary<string, Category> categories)
    {
        var cashWallet = await EnsureWalletAsync(
            db,
            userId,
            "V\u00ed ti\u1ec1n m\u1eb7t",
            "Cash",
            2_000_000m,
            "Chi ti\u00eau h\u1eb1ng ng\u00e0y",
            DateTime.UtcNow.AddDays(-21));

        var bankWallet = await EnsureWalletAsync(
            db,
            userId,
            "TPBank",
            "Bank",
            8_000_000m,
            "Nh\u1eadn l\u01b0\u01a1ng v\u00e0 thanh to\u00e1n online",
            DateTime.UtcNow.AddDays(-21));

        var savingWallet = await EnsureWalletAsync(
            db,
            userId,
            "Qu\u1ef9 ti\u1ebft ki\u1ec7m",
            "Savings",
            5_000_000m,
            "M\u1ee5c ti\u00eau laptop m\u1edbi",
            DateTime.UtcNow.AddDays(-18));

        var today = DateTime.Today;
        var currentMonth = new DateTime(today.Year, today.Month, 1);
        var previousMonth = currentMonth.AddMonths(-1);

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Income", "L\u01b0\u01a1ng"),
            "Income",
            12_000_000m,
            "Nh\u1eadn l\u01b0\u01a1ng th\u00e1ng",
            currentMonth.AddDays(1));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Income", "Th\u01b0\u1edfng"),
            "Income",
            1_500_000m,
            "Th\u01b0\u1edfng d\u1ef1 \u00e1n nh\u1ecf",
            currentMonth.AddDays(6));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            45_000m,
            "B\u00e1nh cu\u1ed1n s\u00e1ng",
            currentMonth.AddDays(1),
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "\u0102n u\u1ed1ng",
            0.82);

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            95_000m,
            "B\u00fan ch\u1ea3 H\u00e0 N\u1ed9i",
            currentMonth.AddDays(2),
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "\u0102n u\u1ed1ng",
            0.91);

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Expense",
            80_000m,
            "\u0110\u1ed5 x\u0103ng xe m\u00e1y",
            currentMonth.AddDays(3),
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Di chuy\u1ec3n",
            0.92);

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "H\u00f3a \u0111\u01a1n"),
            "Expense",
            420_000m,
            "Thanh to\u00e1n ti\u1ec1n \u0111i\u1ec7n n\u01b0\u1edbc",
            currentMonth.AddDays(4));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "Gi\u1ea3i tr\u00ed"),
            "Expense",
            180_000m,
            "\u0110i xem phim cu\u1ed1i tu\u1ea7n",
            currentMonth.AddDays(5));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "H\u1ecdc t\u1eadp"),
            "Expense",
            650_000m,
            "Mua kh\u00f3a h\u1ecdc frontend",
            currentMonth.AddDays(7));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "Mua s\u1eafm"),
            "Expense",
            390_000m,
            "Mua s\u1eafm \u00e1o s\u01a1 mi",
            currentMonth.AddDays(8),
            GetCategoryId(categories, "Expense", "Mua s\u1eafm"),
            "Mua s\u1eafm",
            0.86);

        await EnsureTransactionAsync(
            db,
            userId,
            savingWallet.Id,
            GetCategoryId(categories, "Income", "Thu kh\u00e1c"),
            "Income",
            500_000m,
            "Chuy\u1ec3n qu\u1ef9 ti\u1ebft ki\u1ec7m \u0111\u1ea7u th\u00e1ng",
            currentMonth.AddDays(9));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            85_000m,
            "C\u01a1m t\u1ea5m tr\u01b0a",
            currentMonth.AddDays(10),
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "\u0102n u\u1ed1ng",
            0.9);

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Expense",
            150_000m,
            "\u0110i grab v\u1ec1 qu\u00ea",
            currentMonth.AddDays(12),
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Di chuy\u1ec3n",
            0.88);

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "Nh\u00e0 \u1edf"),
            "Expense",
            1_200_000m,
            "\u0110\u00f3ng ti\u1ec1n tr\u1ecd",
            currentMonth.AddDays(13));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            1_320_000m,
            "T\u1ee5 t\u1eadp sinh nh\u1eadt b\u1ea1n b\u00e8",
            currentMonth.AddDays(14));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Income", "L\u01b0\u01a1ng"),
            "Income",
            11_500_000m,
            "Nh\u1eadn l\u01b0\u01a1ng th\u00e1ng tr\u01b0\u1edbc",
            previousMonth.AddDays(2));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            1_450_000m,
            "\u0102n u\u1ed1ng c\u1ea3 th\u00e1ng tr\u01b0\u1edbc",
            previousMonth.AddDays(18));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Expense",
            760_000m,
            "X\u0103ng xe v\u00e0 g\u1eedi xe th\u00e1ng tr\u01b0\u1edbc",
            previousMonth.AddDays(20));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "H\u1ecdc t\u1eadp"),
            "Expense",
            900_000m,
            "Mua s\u00e1ch chuy\u00ean ng\u00e0nh",
            previousMonth.AddDays(12));

        await SeedPrimaryExtendedActivityAsync(
            db,
            userId,
            cashWallet.Id,
            bankWallet.Id,
            savingWallet.Id,
            categories);

        await EnsureBudgetAsync(
            db,
            userId,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            DateTime.Today.Year,
            DateTime.Today.Month,
            2_000_000m,
            80,
            DateTime.UtcNow.AddDays(-10));

        await EnsureBudgetAsync(
            db,
            userId,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            DateTime.Today.Year,
            DateTime.Today.Month,
            900_000m,
            75,
            DateTime.UtcNow.AddDays(-10));

        await EnsureBudgetAsync(
            db,
            userId,
            GetCategoryId(categories, "Expense", "H\u1ecdc t\u1eadp"),
            DateTime.Today.Year,
            DateTime.Today.Month,
            1_500_000m,
            85,
            DateTime.UtcNow.AddDays(-10));

        await RecalculateWalletBalancesAsync(db, userId);
    }

    private static async Task EnsureMaleDemoProfileAsync(
        FinanceDbContext db,
        int userId,
        IReadOnlyDictionary<string, Category> categories,
        decimal cashInitialBalance,
        decimal bankInitialBalance,
        decimal savingInitialBalance,
        string bankWalletName,
        string savingWalletName,
        int dayShift,
        IReadOnlyList<string> noteSet)
    {
        var cashWallet = await EnsureWalletAsync(
            db,
            userId,
            "V\u00ed ti\u1ec1n m\u1eb7t",
            "Cash",
            cashInitialBalance,
            "Chi ti\u00eau c\u00e1 nh\u00e2n h\u1eb1ng ng\u00e0y",
            DateTime.UtcNow.AddDays(-18 + dayShift));

        var bankWallet = await EnsureWalletAsync(
            db,
            userId,
            bankWalletName,
            "Bank",
            bankInitialBalance,
            "Thanh to\u00e1n online v\u00e0 nh\u1eadn thu nh\u1eadp",
            DateTime.UtcNow.AddDays(-18 + dayShift));

        var savingWallet = await EnsureWalletAsync(
            db,
            userId,
            savingWalletName,
            "Savings",
            savingInitialBalance,
            "Kho\u1ea3n d\u1ef1 ph\u00f2ng ho\u1eb7c m\u1ee5c ti\u00eau ri\u00eang",
            DateTime.UtcNow.AddDays(-16 + dayShift));

        var baseMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Income", "L\u01b0\u01a1ng"),
            "Income",
            7_500_000m,
            noteSet[4],
            baseMonth.AddDays(2 + dayShift));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Expense",
            70_000m,
            noteSet[0],
            baseMonth.AddDays(3 + dayShift),
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Di chuy\u1ec3n",
            0.94);

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            68_000m,
            noteSet[1],
            baseMonth.AddDays(4 + dayShift),
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "\u0102n u\u1ed1ng",
            0.93);

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            42_000m,
            noteSet[2],
            baseMonth.AddDays(5 + dayShift),
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "\u0102n u\u1ed1ng",
            0.88);

        await EnsureTransactionAsync(
            db,
            userId,
            bankWallet.Id,
            GetCategoryId(categories, "Expense", "Mua s\u1eafm"),
            "Expense",
            320_000m,
            noteSet[3],
            baseMonth.AddDays(7 + dayShift),
            GetCategoryId(categories, "Expense", "Mua s\u1eafm"),
            "Mua s\u1eafm",
            0.86);

        await EnsureTransactionAsync(
            db,
            userId,
            savingWallet.Id,
            GetCategoryId(categories, "Income", "Thu kh\u00e1c"),
            "Income",
            350_000m,
            "Chuy\u1ec3n ti\u1ec1n v\u00e0o qu\u1ef9 ri\u00eang",
            baseMonth.AddDays(10 + dayShift));

        await SeedMaleDemoExtendedActivityAsync(
            db,
            userId,
            cashWallet.Id,
            bankWallet.Id,
            savingWallet.Id,
            categories,
            dayShift);

        await EnsureBudgetAsync(
            db,
            userId,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            DateTime.Today.Year,
            DateTime.Today.Month,
            1_400_000m,
            80,
            DateTime.UtcNow.AddDays(-9 + dayShift));

        await EnsureBudgetAsync(
            db,
            userId,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            DateTime.Today.Year,
            DateTime.Today.Month,
            700_000m,
            75,
            DateTime.UtcNow.AddDays(-9 + dayShift));

        await RecalculateWalletBalancesAsync(db, userId);
    }

    private static async Task EnsureShowcaseYearProfileAsync(
        FinanceDbContext db,
        int userId,
        IReadOnlyDictionary<string, Category> categories)
    {
        var cashWallet = await db.Wallets.FirstAsync(x => x.UserId == userId && x.Name == "V\u00ed ti\u1ec1n m\u1eb7t");
        var bankWallet = await db.Wallets.FirstAsync(x => x.UserId == userId && x.Name == "TPBank");
        var savingWallet = await db.Wallets.FirstAsync(x => x.UserId == userId && x.Name == "Qu\u1ef9 ti\u1ebft ki\u1ec7m");

        var bankAssignedNotes = new[]
        {
            "Tong hop an uong ",
            "Di lai va gui xe "
        };

        var showcaseExpenseTransfers = await db.Transactions
            .Where(x => x.UserId == userId &&
                        x.Type == "Expense" &&
                        bankAssignedNotes.Any(prefix => x.Note.StartsWith(prefix)) &&
                        x.WalletId != bankWallet.Id)
            .ToListAsync();

        foreach (var item in showcaseExpenseTransfers)
        {
            item.WalletId = bankWallet.Id;
        }

        if (showcaseExpenseTransfers.Count > 0)
        {
            await db.SaveChangesAsync();
        }

        await EnsureTransactionAsync(
            db,
            userId,
            cashWallet.Id,
            GetCategoryId(categories, "Income", "Thu kh\u00e1c"),
            "Income",
            3_000_000m,
            "Bo sung quy tien mat de demo du lieu 12 thang",
            new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-11).AddDays(1).AddHours(9));

        var salaryCategoryId = GetCategoryId(categories, "Income", "L\u01b0\u01a1ng");
        var bonusCategoryId = GetCategoryId(categories, "Income", "Th\u01b0\u1edfng");
        var otherIncomeCategoryId = GetCategoryId(categories, "Income", "Thu kh\u00e1c");
        var giftCategoryId = GetCategoryId(categories, "Income", "Qu\u00e0 t\u1eb7ng");
        var foodCategoryId = GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng");
        var transportCategoryId = GetCategoryId(categories, "Expense", "Di chuy\u1ec3n");
        var housingCategoryId = GetCategoryId(categories, "Expense", "Nh\u00e0 \u1edf");
        var billCategoryId = GetCategoryId(categories, "Expense", "H\u00f3a \u0111\u01a1n");
        var studyCategoryId = GetCategoryId(categories, "Expense", "H\u1ecdc t\u1eadp");
        var shoppingCategoryId = GetCategoryId(categories, "Expense", "Mua s\u1eafm");
        var entertainmentCategoryId = GetCategoryId(categories, "Expense", "Gi\u1ea3i tr\u00ed");
        var healthCategoryId = GetCategoryId(categories, "Expense", "S\u1ee9c kh\u1ecfe");

        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        for (var offset = 11; offset >= 0; offset--)
        {
            var monthStart = currentMonth.AddMonths(-offset);
            var monthTag = monthStart.ToString("MM/yyyy");
            var monthNumber = monthStart.Month;
            var salaryAmount = 11_200_000m + (monthNumber % 4) * 220_000m;
            var bonusAmount = 650_000m + (monthNumber % 3) * 140_000m;
            var foodAmount = 880_000m + (monthNumber % 5) * 75_000m;
            var transportAmount = 340_000m + (monthNumber % 4) * 40_000m;
            var housingAmount = 1_650_000m + (monthNumber % 2) * 180_000m;
            var flexibleExpenseAmount = 460_000m + (monthNumber % 4) * 120_000m;
            var wellnessAmount = 210_000m + (monthNumber % 3) * 60_000m;
            var savingAmount = 520_000m + (monthNumber % 4) * 110_000m;

            await EnsureTransactionAsync(
                db,
                userId,
                bankWallet.Id,
                salaryCategoryId,
                "Income",
                salaryAmount,
                $"Luong cong viec chinh {monthTag}",
                monthStart.AddDays(1).AddHours(8));

            await EnsureTransactionAsync(
                db,
                userId,
                bankWallet.Id,
                bonusCategoryId,
                "Income",
                bonusAmount,
                $"Thu nhap freelance {monthTag}",
                monthStart.AddDays(6).AddHours(9));

            await EnsureTransactionAsync(
                db,
                userId,
                bankWallet.Id,
                foodCategoryId,
                "Expense",
                foodAmount,
                $"Tong hop an uong {monthTag}",
                monthStart.AddDays(4).AddHours(12),
                foodCategoryId,
                "\u0102n u\u1ed1ng",
                0.93);

            await EnsureTransactionAsync(
                db,
                userId,
                bankWallet.Id,
                transportCategoryId,
                "Expense",
                transportAmount,
                $"Di lai va gui xe {monthTag}",
                monthStart.AddDays(8).AddHours(7),
                transportCategoryId,
                "Di chuy\u1ec3n",
                0.89);

            await EnsureTransactionAsync(
                db,
                userId,
                bankWallet.Id,
                monthNumber % 2 == 0 ? housingCategoryId : billCategoryId,
                "Expense",
                housingAmount,
                monthNumber % 2 == 0
                    ? $"Tien tro va sinh hoat {monthTag}"
                    : $"Tien dien nuoc internet {monthTag}",
                monthStart.AddDays(12).AddHours(20));

            await EnsureTransactionAsync(
                db,
                userId,
                bankWallet.Id,
                monthNumber % 2 == 0 ? studyCategoryId : shoppingCategoryId,
                "Expense",
                flexibleExpenseAmount,
                monthNumber % 2 == 0
                    ? $"Tai lieu va hoc phi bo sung {monthTag}"
                    : $"Mua sam phuc vu hoc tap {monthTag}",
                monthStart.AddDays(18).AddHours(19));

            await EnsureTransactionAsync(
                db,
                userId,
                bankWallet.Id,
                monthNumber % 2 == 0 ? entertainmentCategoryId : healthCategoryId,
                "Expense",
                wellnessAmount,
                monthNumber % 2 == 0
                    ? $"Giai tri cuoi thang {monthTag}"
                    : $"Cham soc suc khoe {monthTag}",
                monthStart.AddDays(23).AddHours(18));

            await EnsureTransactionAsync(
                db,
                userId,
                savingWallet.Id,
                monthNumber % 2 == 0 ? giftCategoryId : otherIncomeCategoryId,
                "Income",
                savingAmount,
                $"Bo sung quy tiet kiem {monthTag}",
                monthStart.AddDays(25).AddHours(9));

            await EnsureBudgetAsync(
                db,
                userId,
                foodCategoryId,
                monthStart.Year,
                monthStart.Month,
                2_300_000m + (monthNumber % 3) * 120_000m,
                80,
                monthStart.AddDays(1));

            await EnsureBudgetAsync(
                db,
                userId,
                transportCategoryId,
                monthStart.Year,
                monthStart.Month,
                950_000m + (monthNumber % 2) * 70_000m,
                75,
                monthStart.AddDays(1));

            await EnsureBudgetAsync(
                db,
                userId,
                monthNumber % 2 == 0 ? studyCategoryId : shoppingCategoryId,
                monthStart.Year,
                monthStart.Month,
                1_600_000m + (monthNumber % 4) * 110_000m,
                85,
                monthStart.AddDays(1));
        }

        await RecalculateWalletBalancesAsync(db, userId);
    }

    private static async Task SeedPrimaryExtendedActivityAsync(
        FinanceDbContext db,
        int userId,
        int cashWalletId,
        int bankWalletId,
        int savingWalletId,
        IReadOnlyDictionary<string, Category> categories)
    {
        var today = DateTime.Today;
        var currentMonth = new DateTime(today.Year, today.Month, 1);
        var previousMonth = currentMonth.AddMonths(-1);

        await EnsureTransactionAsync(
            db,
            userId,
            cashWalletId,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            58_000m,
            "Banh mi trung sau gio hoc",
            today.AddDays(-7).Date.AddHours(7));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWalletId,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            72_000m,
            "Com ga xoi mo buoi trua",
            today.AddDays(-6).Date.AddHours(12));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWalletId,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Expense",
            120_000m,
            "Gui xe va do xang dau tuan",
            today.AddDays(-5).Date.AddHours(8),
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Di chuy\u1ec3n",
            0.89);

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "H\u00f3a \u0111\u01a1n"),
            "Expense",
            315_000m,
            "Dong tien internet va dien thoai",
            today.AddDays(-4).Date.AddHours(20));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "S\u1ee9c kh\u1ecfe"),
            "Expense",
            240_000m,
            "Mua thuoc va vitamin",
            today.AddDays(-3).Date.AddHours(18));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "Mua s\u1eafm"),
            "Expense",
            680_000m,
            "Mua ban phim co phuc vu hoc tap",
            today.AddDays(-2).Date.AddHours(21));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWalletId,
            GetCategoryId(categories, "Expense", "Gi\u1ea3i tr\u00ed"),
            "Expense",
            95_000m,
            "Cafe lam viec nhom",
            today.AddDays(-1).Date.AddHours(19));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Income", "B\u00e1n h\u00e0ng"),
            "Income",
            420_000m,
            "Ban lai sach cu va do dung hoc tap",
            currentMonth.AddDays(9).AddHours(10));

        await EnsureTransactionAsync(
            db,
            userId,
            savingWalletId,
            GetCategoryId(categories, "Income", "Qu\u00e0 t\u1eb7ng"),
            "Income",
            650_000m,
            "Nguoi nha gui them vao quy tiet kiem",
            currentMonth.AddDays(11).AddHours(9));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "Nh\u00e0 \u1edf"),
            "Expense",
            450_000m,
            "Dong tien wifi va ve sinh thang nay",
            currentMonth.AddDays(12).AddHours(20));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "Gi\u1ea3i tr\u00ed"),
            "Expense",
            260_000m,
            "Lien hoan nhom do an cuoi thang",
            previousMonth.AddDays(15).AddHours(19));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Income", "Thu kh\u00e1c"),
            "Income",
            900_000m,
            "Lam them su kien cuoi thang truoc",
            previousMonth.AddDays(21).AddHours(8));
    }

    private static async Task SeedMaleDemoExtendedActivityAsync(
        FinanceDbContext db,
        int userId,
        int cashWalletId,
        int bankWalletId,
        int savingWalletId,
        IReadOnlyDictionary<string, Category> categories,
        int dayShift)
    {
        var today = DateTime.Today;
        var currentMonth = new DateTime(today.Year, today.Month, 1);
        var previousMonth = currentMonth.AddMonths(-1);

        await EnsureTransactionAsync(
            db,
            userId,
            cashWalletId,
            GetCategoryId(categories, "Expense", "\u0102n u\u1ed1ng"),
            "Expense",
            55_000m,
            "Tra sua sau gio hoc",
            today.AddDays(-(6 + dayShift % 3)).Date.AddHours(16));

        await EnsureTransactionAsync(
            db,
            userId,
            cashWalletId,
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Expense",
            38_000m,
            "Gui xe ca tuan",
            today.AddDays(-(5 + dayShift % 2)).Date.AddHours(8),
            GetCategoryId(categories, "Expense", "Di chuy\u1ec3n"),
            "Di chuy\u1ec3n",
            0.84);

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "H\u00f3a \u0111\u01a1n"),
            "Expense",
            245_000m,
            "Dong tien internet va dien thoai",
            currentMonth.AddDays(8 + dayShift).AddHours(20));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "H\u1ecdc t\u1eadp"),
            "Expense",
            275_000m,
            "In tai lieu va photo bai tap lon",
            currentMonth.AddDays(11 + dayShift).AddHours(14));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "Nh\u00e0 \u1edf"),
            "Expense",
            920_000m,
            "Dong tien phong giua thang",
            currentMonth.AddDays(13 + dayShift).AddHours(19));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "S\u1ee9c kh\u1ecfe"),
            "Expense",
            180_000m,
            "Mua thuoc cam va vitamin",
            currentMonth.AddDays(15 + dayShift).AddHours(18));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Income", "Thu kh\u00e1c"),
            "Income",
            620_000m,
            "Lam them cuoi tuan",
            currentMonth.AddDays(17 + dayShift).AddHours(9));

        await EnsureTransactionAsync(
            db,
            userId,
            savingWalletId,
            GetCategoryId(categories, "Income", "Qu\u00e0 t\u1eb7ng"),
            "Income",
            300_000m,
            "Nguoi nha gui them vao quy rieng",
            currentMonth.AddDays(19 + dayShift).AddHours(8));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Expense", "Gi\u1ea3i tr\u00ed"),
            "Expense",
            230_000m,
            "Lien hoan voi ban cung lop",
            previousMonth.AddDays(10 + dayShift).AddHours(19));

        await EnsureTransactionAsync(
            db,
            userId,
            bankWalletId,
            GetCategoryId(categories, "Income", "Th\u01b0\u1edfng"),
            "Income",
            450_000m,
            "Thuong chuyen can thang truoc",
            previousMonth.AddDays(22).AddHours(9));
    }

    private static async Task<Wallet> EnsureWalletAsync(
        FinanceDbContext db,
        int userId,
        string name,
        string type,
        decimal initialBalance,
        string note,
        DateTime createdAt)
    {
        var wallet = await db.Wallets.FirstOrDefaultAsync(x => x.UserId == userId && x.Name == name);
        if (wallet is not null)
        {
            wallet.Type = type;
            wallet.InitialBalance = initialBalance;
            wallet.Note = note;
            if (wallet.CreatedAt == default)
            {
                wallet.CreatedAt = createdAt;
            }

            await db.SaveChangesAsync();
            return wallet;
        }

        wallet = new Wallet
        {
            UserId = userId,
            Name = name,
            Type = type,
            InitialBalance = initialBalance,
            CurrentBalance = initialBalance,
            Note = note,
            CreatedAt = createdAt
        };

        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return wallet;
    }

    private static async Task EnsureTransactionAsync(
        FinanceDbContext db,
        int userId,
        int walletId,
        int categoryId,
        string type,
        decimal amount,
        string note,
        DateTime occurredOn,
        int? aiSuggestedCategoryId = null,
        string? aiSuggestedLabel = null,
        double aiConfidence = 0)
    {
        var exists = await db.Transactions.AnyAsync(x =>
            x.UserId == userId &&
            x.WalletId == walletId &&
            x.CategoryId == categoryId &&
            x.Type == type &&
            x.Amount == amount &&
            x.Note == note &&
            x.OccurredOn == occurredOn);

        if (exists)
        {
            return;
        }

        db.Transactions.Add(new WalletTransaction
        {
            UserId = userId,
            WalletId = walletId,
            CategoryId = categoryId,
            Type = type,
            Amount = amount,
            Note = note,
            OccurredOn = occurredOn,
            CreatedAt = occurredOn.AddHours(1),
            AiSuggestedCategoryId = aiSuggestedCategoryId,
            AiSuggestedLabel = aiSuggestedLabel,
            AiConfidence = aiConfidence
        });

        await db.SaveChangesAsync();
    }

    private static async Task EnsureBudgetAsync(
        FinanceDbContext db,
        int userId,
        int categoryId,
        int year,
        int month,
        decimal limitAmount,
        int thresholdPercent,
        DateTime createdAt)
    {
        var budget = await db.Budgets.FirstOrDefaultAsync(x =>
            x.UserId == userId &&
            x.CategoryId == categoryId &&
            x.Year == year &&
            x.Month == month);

        if (budget is null)
        {
            db.Budgets.Add(new Budget
            {
                UserId = userId,
                CategoryId = categoryId,
                Year = year,
                Month = month,
                LimitAmount = limitAmount,
                AlertThresholdPercent = thresholdPercent,
                CreatedAt = createdAt
            });

            await db.SaveChangesAsync();
            return;
        }

        budget.LimitAmount = limitAmount;
        budget.AlertThresholdPercent = thresholdPercent;
        await db.SaveChangesAsync();
    }

    private static async Task RecalculateWalletBalancesAsync(FinanceDbContext db, int userId)
    {
        var wallets = await db.Wallets.Where(x => x.UserId == userId).ToListAsync();
        var transactions = await db.Transactions.Where(x => x.UserId == userId).ToListAsync();

        foreach (var wallet in wallets)
        {
            var walletTransactions = transactions.Where(x => x.WalletId == wallet.Id);
            var income = walletTransactions.Where(x => x.Type == "Income").Sum(x => x.Amount);
            var expense = walletTransactions.Where(x => x.Type == "Expense").Sum(x => x.Amount);
            wallet.CurrentBalance = wallet.InitialBalance + income - expense;
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedAlertsAsync(FinanceDbContext db, int userId, int foodCategoryId)
    {
        if (await db.BudgetAlerts.AnyAsync(x => x.UserId == userId))
        {
            return;
        }

        var budget = await db.Budgets.FirstAsync(x => x.UserId == userId && x.CategoryId == foodCategoryId);
        var latestFoodExpense = await db.Transactions
            .Where(x => x.UserId == userId && x.CategoryId == foodCategoryId && x.Type == "Expense")
            .OrderByDescending(x => x.OccurredOn)
            .FirstAsync();

        db.BudgetAlerts.Add(new BudgetAlert
        {
            UserId = userId,
            BudgetId = budget.Id,
            WalletTransactionId = latestFoodExpense.Id,
            Message = "Ng\u00e2n s\u00e1ch \u0102n u\u1ed1ng \u0111\u00e3 ti\u1ebfn g\u1ea7n m\u1ee9c c\u1ea3nh b\u00e1o trong th\u00e1ng n\u00e0y.",
            SpentAmount = 1_590_000m,
            LimitAmount = budget.LimitAmount,
            UsagePercent = 79.5,
            CreatedAt = DateTime.UtcNow.AddHours(-6)
        });

        await db.SaveChangesAsync();
    }

    private static async Task SeedLogsAsync(FinanceDbContext db, int userId)
    {
        if (await db.SystemLogs.AnyAsync())
        {
            return;
        }

        db.SystemLogs.AddRange(
            new SystemLog
            {
                UserId = userId,
                Level = "Info",
                Action = "SeedDemoData",
                Message = "\u0110\u00e3 kh\u1edfi t\u1ea1o d\u1eef li\u1ec7u m\u1eabu cho c\u00e1c t\u00e0i kho\u1ea3n demo.",
                CreatedAt = DateTime.UtcNow.AddHours(-10)
            },
            new SystemLog
            {
                UserId = userId,
                Level = "Info",
                Action = "WeeklyReport",
                Message = "\u0110\u01b0a b\u00e1o c\u00e1o tu\u1ea7n v\u00e0o h\u00e0ng \u0111\u1ee3i g\u1eedi email.",
                CreatedAt = DateTime.UtcNow.AddHours(-8)
            },
            new SystemLog
            {
                UserId = userId,
                Level = "Warning",
                Action = "BudgetAlert",
                Message = "Ng\u00e2n s\u00e1ch \u0102n u\u1ed1ng \u0111\u00e3 ch\u1ea1m m\u1ed1c c\u1ea3nh b\u00e1o.",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            });

        await db.SaveChangesAsync();
    }

    private static async Task<Dictionary<string, Category>> BuildCategoryLookupAsync(FinanceDbContext db)
    {
        var categories = await db.Categories.ToListAsync();
        return categories.ToDictionary(
            x => BuildCategoryKey(x.TransactionType, x.Name),
            x => x,
            StringComparer.OrdinalIgnoreCase);
    }

    private static int GetCategoryId(IReadOnlyDictionary<string, Category> categories, string transactionType, string name)
    {
        return categories[BuildCategoryKey(transactionType, name)].Id;
    }

    private static string BuildCategoryKey(string transactionType, string name)
    {
        return $"{transactionType}:{NormalizeSeedText(name)}";
    }

    private static string NormalizeSeedText(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(ch switch
            {
                '\u0111' => 'd',
                '\u0110' => 'D',
                _ => char.ToLowerInvariant(ch)
            });
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).Trim();
    }

    private sealed record DemoUserSeed(
        string Username,
        string Email,
        string Password,
        string DisplayName,
        bool IsActive,
        bool IsEmailVerified,
        DateTime CreatedAt);
}
