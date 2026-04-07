using DoAnLTW.Web.Data;
using DoAnLTW.Web.Hubs;
using DoAnLTW.Web.Models.Options;
using DoAnLTW.Web.Services.Auth;
using DoAnLTW.Web.Services.Email;
using DoAnLTW.Web.Services.Finance;
using DoAnLTW.Web.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
var seedDemoData = builder.Configuration.GetValue<bool>("SeedDemoData");
var seedShowcaseUserEmail = builder.Configuration["SeedShowcaseUserEmail"];

if (string.IsNullOrWhiteSpace(defaultConnection))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured. Set it via User Secrets or environment variables.");
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

var dataProtectionKeysPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "App_Data",
    "DataProtectionKeys");

Directory.CreateDirectory(dataProtectionKeysPath);

builder.Services.AddDataProtection()
    .SetApplicationName("DoAnLTW")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection("AdminAccount"));

builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseSqlServer(defaultConnection));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<CookieAuthService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<CategorizationService>();
builder.Services.AddScoped<ForecastService>();
builder.Services.AddScoped<BudgetMonitorService>();
builder.Services.AddScoped<WalletBalanceMonitorService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddScoped<AvatarStorageService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddHostedService<QueuedEmailBackgroundService>();
builder.Services.AddHostedService<WeeklyReportHostedService>();

var app = builder.Build();

await SeedData.InitializeAsync(app.Services, seedDemoData, seedShowcaseUserEmail);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/ErrorStatus?code=500");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/Home/ErrorStatus", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<BudgetHub>("/budgetHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
