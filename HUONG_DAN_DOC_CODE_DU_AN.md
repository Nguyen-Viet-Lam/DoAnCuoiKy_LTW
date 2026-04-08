# Hướng Dẫn Đọc Code Dự Án DoAnLTW

Tài liệu này được viết để phục vụ 3 mục tiêu:

- đọc code nhanh và biết chức năng nằm ở đâu
- hiểu luồng xử lý từ request đến database, service, UI
- luyện vấn đáp theo kiểu giảng viên môn Lập trình Web hỏi xoáy vào chính đồ án

Tài liệu ưu tiên trả lời 5 câu:

1. Dự án dùng công nghệ gì
2. Database có những bảng nào và quan hệ ra sao
3. Mỗi chức năng nằm ở file nào
4. Luồng xử lý chính chạy qua những bước nào
5. Công thức, thuật toán, service nào là phần đáng hỏi nhất khi bảo vệ

## 1. Bức tranh tổng thể

Đây là một ứng dụng web quản lý chi tiêu cá nhân viết bằng ASP.NET Core MVC trên .NET 8. Hệ thống dùng SQL Server để lưu dữ liệu, Entity Framework Core để thao tác database, Cookie Authentication để đăng nhập, SignalR để cảnh báo realtime, SMTP để gửi OTP và báo cáo, ClosedXML để xuất Excel.

Nhóm chức năng chính:

- xác thực: đăng ký, đăng nhập, OTP, quên mật khẩu
- tài chính cá nhân: ví, giao dịch, ngân sách
- phân tích: dashboard, báo cáo, dự báo chi tiêu
- hỗ trợ thông minh: gợi ý danh mục theo từ khóa và hành vi user
- vận hành: email queue, background service, admin, audit log

## 2. Công nghệ và thư viện

### 2.1. Package và framework chính

Nguồn đọc:

- `DoAnLTW.Web/DoAnLTW.Web.csproj`
- `DoAnLTW.Web/Program.cs`

Các thành phần quan trọng:

- `.NET 8`: nền tảng chạy ứng dụng
- `ASP.NET Core MVC`: tổ chức theo Model - View - Controller
- `Entity Framework Core + SQL Server`: ORM và database
- `Cookie Authentication`: xác thực phiên người dùng
- `SignalR`: đẩy cảnh báo thời gian thực
- `ClosedXML`: xuất file Excel
- `BackgroundService`: xử lý tác vụ nền
- `System.Threading.Channels`: hàng đợi email trong memory
- `Bootstrap 5`, `Bootstrap Icons`, `jQuery`, `site.js`, `site.css`: giao diện và tương tác frontend

### 2.2. App khởi động ở đâu

Nguồn đọc:

- `DoAnLTW.Web/Program.cs`

Những dòng quan trọng trong `Program.cs`:

- đọc `DefaultConnection`
- bật MVC bằng `AddControllersWithViews()`
- bật SignalR bằng `AddSignalR()`
- bật Cookie Authentication
- đăng ký `FinanceDbContext`
- đăng ký toàn bộ service nghiệp vụ
- đăng ký `EmailQueue` là `Singleton`
- đăng ký 2 `HostedService`:
  - `QueuedEmailBackgroundService`
  - `WeeklyReportHostedService`
- chạy `SeedData.InitializeAsync(...)`
- map `BudgetHub` tại `/budgetHub`

## 3. Cấu trúc thư mục nên đọc

### 3.1. Thư mục quan trọng nhất

- `DoAnLTW.Web/Program.cs`
- `DoAnLTW.Web/Data`
- `DoAnLTW.Web/Models/Entities`
- `DoAnLTW.Web/Controllers`
- `DoAnLTW.Web/Services`
- `DoAnLTW.Web/Views`
- `DoAnLTW.Web/wwwroot`

### 3.2. Nếu chỉ có 15 phút để nắm dự án

Đọc theo thứ tự này:

1. `DoAnLTW.Web/Program.cs`
2. `DoAnLTW.Web/Data/FinanceDbContext.cs`
3. `DoAnLTW.Web/Models/Entities/IdentityEntities.cs`
4. `DoAnLTW.Web/Models/Entities/FinanceEntities.cs`
5. `DoAnLTW.Web/Controllers/AuthController.cs`
6. `DoAnLTW.Web/Controllers/TransactionsController.cs`
7. `DoAnLTW.Web/Services/Finance/CategorizationService.cs`
8. `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs`
9. `DoAnLTW.Web/Services/Finance/ForecastService.cs`
10. `DoAnLTW.Web/Controllers/DashboardController.cs`
11. `DoAnLTW.Web/Services/Finance/ReportService.cs`
12. `DoAnLTW.Web/Controllers/AdminController.cs`

## 4. Database: bảng, quan hệ, ràng buộc

### 4.1. DbContext

Nguồn đọc:

- `DoAnLTW.Web/Data/FinanceDbContext.cs`

DbSet chính:

- `Roles`
- `Users`
- `EmailOtps`
- `Wallets`
- `Categories`
- `Transactions`
- `Budgets`
- `UserPersonalKeywords`
- `BudgetAlerts`
- `SystemLogs`
- `ReportDispatchLogs`

### 4.2. Các entity chính

Nguồn đọc:

- `DoAnLTW.Web/Models/Entities/IdentityEntities.cs`
- `DoAnLTW.Web/Models/Entities/FinanceEntities.cs`
- `DoAnLTW.Web/Models/Entities/SystemEntities.cs`

Ý nghĩa từng bảng:

- `Role`: vai trò như `Admin`, `User`
- `AppUser`: tài khoản người dùng
- `EmailOtp`: OTP cho đăng ký và đặt lại mật khẩu
- `Wallet`: ví tiền
- `Category`: danh mục thu chi
- `WalletTransaction`: giao dịch tài chính
- `Budget`: ngân sách theo danh mục, tháng, năm
- `BudgetAlert`: cảnh báo vượt ngân sách
- `UserPersonalKeyword`: từ khóa cá nhân cho gợi ý danh mục
- `SystemLog`: log hoạt động hệ thống
- `ReportDispatchLog`: log gửi báo cáo

### 4.3. Quan hệ dữ liệu quan trọng

- Một `Role` có nhiều `AppUser`
- Một `AppUser` có nhiều `Wallet`
- Một `AppUser` có nhiều `WalletTransaction`
- Một `AppUser` có nhiều `Budget`
- Một `AppUser` có nhiều `BudgetAlert`
- Một `AppUser` có nhiều `UserPersonalKeyword`
- Một `Wallet` có nhiều `WalletTransaction`
- Một `Category` có nhiều `WalletTransaction`
- Một `Category` có nhiều `Budget`
- Một `Budget` có nhiều `BudgetAlert`

### 4.4. Ràng buộc và index đáng nhớ

Nguồn đọc:

- `DoAnLTW.Web/Data/FinanceDbContext.cs`

Các ràng buộc chính:

- `Role.Name` unique
- `AppUser.Username` unique
- `AppUser.Email` unique
- `Budget(UserId, CategoryId, Year, Month)` unique
- `UserPersonalKeyword(UserId, Keyword)` unique
- `ReportDispatchLog(UserId, ReportType, PeriodKey)` unique
- `EmailOtp(Email, Purpose, IsUsed)` có index để lookup nhanh

### 4.5. Hành vi xóa quan trọng

- `AppUser -> Role`: `Restrict`
- `Wallet -> User`: `Cascade`
- `WalletTransaction -> Wallet`: `Restrict`
- `WalletTransaction -> Category`: `Restrict`
- `Budget -> User`: `Cascade`
- `BudgetAlert -> User`: `NoAction`
- `BudgetAlert -> Budget`: `NoAction`
- `BudgetAlert -> WalletTransaction`: `Restrict`
- `SystemLog -> User`: `SetNull`

### 4.6. Cây quan hệ đơn giản để minh họa

```text
[Role]
   |
   | 1 - n
   v
[AppUser] ------------------------+
   |                              |
   | 1 - n                        | 1 - n
   v                              v
[Wallet]                      [Budget] --------> [BudgetAlert]
   |                              |
   | 1 - n                        | n - 1
   v                              v
[WalletTransaction] --------> [Category]

[AppUser] --------> [EmailOtp]
[AppUser] --------> [UserPersonalKeyword] --------> [Category]
[AppUser] --------> [SystemLog]
[AppUser] --------> [ReportDispatchLog]
```

## 5. Map chức năng: code nằm ở đâu

### 5.1. Controller

- `AuthController.cs`: đăng ký, đăng nhập, OTP, quên mật khẩu
- `WalletsController.cs`: ví
- `TransactionsController.cs`: giao dịch, gợi ý danh mục
- `BudgetsController.cs`: ngân sách
- `DashboardController.cs`: dashboard user
- `ReportsController.cs`: báo cáo, Excel, gửi email
- `ProfileController.cs`: hồ sơ cá nhân, avatar, đổi mật khẩu
- `AdminController.cs`: dashboard admin, user, category, logs
- `HomeController.cs`: trang chủ và error status
- `AppControllerBase.cs`: helper dùng chung như `CurrentUserId`, flash message

### 5.2. Service

- `PasswordService.cs`: băm và verify mật khẩu
- `CookieAuthService.cs`: sign in / sign out với cookie
- `OtpService.cs`: gửi và xác minh OTP
- `CategorizationService.cs`: gợi ý danh mục
- `ForecastService.cs`: dự báo chi tiêu cuối tháng
- `BudgetMonitorService.cs`: kiểm tra vượt ngân sách, lưu alert, bắn SignalR, gửi mail
- `WalletBalanceMonitorService.cs`: cảnh báo ví xuống thấp
- `ReportService.cs`: tổng hợp báo cáo, xuất Excel, build email
- `AuditLogService.cs`: ghi log hệ thống
- `AvatarStorageService.cs`: lưu avatar
- `EmailQueue.cs`: hàng đợi email
- `QueuedEmailBackgroundService.cs`: worker gửi email nền
- `SmtpEmailSender.cs`: gửi mail SMTP hoặc lưu bản dev
- `WeeklyReportHostedService.cs`: job báo cáo tuần

### 5.3. Bản đồ file, method và line number quan trọng

Phần này dùng để trả lời kiểu câu hỏi: “nó nằm ở file nào, method nào, dòng nào”.

**Startup và hạ tầng**

- `DoAnLTW.Web/Program.cs:28`: bật MVC bằng `AddControllersWithViews()`
- `DoAnLTW.Web/Program.cs:29`: bật SignalR bằng `AddSignalR()`
- `DoAnLTW.Web/Program.cs:49`: bật Cookie Authentication
- `DoAnLTW.Web/Program.cs:60-70`: đăng ký service nghiệp vụ
- `DoAnLTW.Web/Program.cs:71`: đăng ký `EmailQueue` là `Singleton`
- `DoAnLTW.Web/Program.cs:72-73`: đăng ký background service
- `DoAnLTW.Web/Program.cs:77`: chạy `SeedData.InitializeAsync(...)`
- `DoAnLTW.Web/Program.cs:92`: map hub `/budgetHub`
- `DoAnLTW.Web/Data/SeedData.cs:8`: entry point seed dữ liệu

**Database**

- `DoAnLTW.Web/Data/FinanceDbContext.cs:12-22`: khai báo toàn bộ `DbSet`
- `DoAnLTW.Web/Data/FinanceDbContext.cs:29-56`: index và unique index
- `DoAnLTW.Web/Data/FinanceDbContext.cs:59-152`: quan hệ và `DeleteBehavior`

**Auth**

- `DoAnLTW.Web/Controllers/AuthController.cs:49`: `Login(...)`
- `DoAnLTW.Web/Controllers/AuthController.cs:93`: `Register(...)`
- `DoAnLTW.Web/Controllers/AuthController.cs:153`: `VerifyOtp(...)`
- `DoAnLTW.Web/Controllers/AuthController.cs:184`: `ResendOtp(...)`
- `DoAnLTW.Web/Controllers/AuthController.cs:202`: `ForgotPassword(...)`
- `DoAnLTW.Web/Controllers/AuthController.cs:229`: `ResetPassword(...)`
- `DoAnLTW.Web/Services/Security/PasswordService.cs:11`: `Hash(...)`
- `DoAnLTW.Web/Services/Security/PasswordService.cs:18`: `Verify(...)`
- `DoAnLTW.Web/Services/Auth/OtpService.cs:21`: `SendOtpAsync(...)`
- `DoAnLTW.Web/Services/Auth/OtpService.cs:59`: `VerifyAsync(...)`
- `DoAnLTW.Web/Services/Auth/CookieAuthService.cs:10`: `SignInAsync(...)`
- `DoAnLTW.Web/Services/Auth/CookieAuthService.cs:34`: `SignOutAsync(...)`

**User identity helper**

- `DoAnLTW.Web/Controllers/AppControllerBase.cs:11`: `CurrentUserId`
- `DoAnLTW.Web/Controllers/AppControllerBase.cs:13`: `SetBudgetAlertWarning(...)`
- `DoAnLTW.Web/Extensions/ClaimsPrincipalExtensions.cs:7`: `GetUserId(...)`
- `DoAnLTW.Web/Extensions/ClaimsPrincipalExtensions.cs:13`: `GetDisplayName(...)`

**Ví**

- `DoAnLTW.Web/Controllers/WalletsController.cs:28`: `Index(...)`
- `DoAnLTW.Web/Controllers/WalletsController.cs:73`: `Save(...)`
- `DoAnLTW.Web/Controllers/WalletsController.cs:124`: `Delete(...)`
- `DoAnLTW.Web/Controllers/WalletsController.cs:148`: `BuildPageModelAsync(...)`

**Giao dịch**

- `DoAnLTW.Web/Controllers/TransactionsController.cs:34`: `Index(...)`
- `DoAnLTW.Web/Controllers/TransactionsController.cs:59`: `Save(...)`
- `DoAnLTW.Web/Controllers/TransactionsController.cs:193`: `Delete(...)`
- `DoAnLTW.Web/Controllers/TransactionsController.cs:222`: `SuggestCategory(...)`
- `DoAnLTW.Web/Controllers/TransactionsController.cs:242`: `BuildPageModelAsync(...)`

**Gợi ý danh mục**

- `DoAnLTW.Web/Services/Finance/CategorizationService.cs:55`: `SuggestAsync(...)`
- `DoAnLTW.Web/Services/Finance/CategorizationService.cs:109`: `LearnAsync(...)`
- `DoAnLTW.Web/Services/Finance/CategorizationService.cs:159`: `Normalize(...)`
- `DoAnLTW.Web/Services/Finance/CategorizationService.cs:170`: `ExtractKeywords(...)`
- `DoAnLTW.Web/Services/Finance/CategorizationService.cs:176`: `RemoveDiacritics(...)`

**Ngân sách và cảnh báo**

- `DoAnLTW.Web/Controllers/BudgetsController.cs:28`: `Index(...)`
- `DoAnLTW.Web/Controllers/BudgetsController.cs:55`: `Save(...)`
- `DoAnLTW.Web/Controllers/BudgetsController.cs:120`: `Delete(...)`
- `DoAnLTW.Web/Controllers/BudgetsController.cs:138`: `BuildPageModelAsync(...)`
- `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:40`: `CheckAndNotifyAsync(...)`
- `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:142`: `CheckBudgetStatusAsync(...)`
- `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:168`: `TrySendBudgetAlertEmailAsync(...)`
- `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:213`: `BuildBudgetAlertEmailHtml(...)`
- `DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs:23`: `NotifyIfThresholdCrossedAsync(...)`

**Dashboard và forecast**

- `DoAnLTW.Web/Controllers/DashboardController.cs:27`: `Index(...)`
- `DoAnLTW.Web/Controllers/DashboardController.cs:185`: `MarkAlertAsRead(...)`
- `DoAnLTW.Web/Controllers/DashboardController.cs:201`: `MarkAllAlertsAsRead(...)`
- `DoAnLTW.Web/Controllers/DashboardController.cs:220`: `BuildBudgetProgressAsync(...)`
- `DoAnLTW.Web/Services/Finance/ForecastService.cs:15`: `CalculateMonthEndExpenseForecastAsync(...)`
- `DoAnLTW.Web/Services/Finance/ForecastService.cs:98`: `CalculateSpikeThreshold(...)`
- `DoAnLTW.Web/Services/Finance/ForecastService.cs:124`: `GetPercentile(...)`
- `DoAnLTW.Web/Services/Finance/ForecastService.cs:142`: `GetAverageMonthlyExpenseAsync(...)`

**Báo cáo và Excel**

- `DoAnLTW.Web/Controllers/ReportsController.cs:26`: `Index(...)`
- `DoAnLTW.Web/Controllers/ReportsController.cs:62`: `ExportExcel(...)`
- `DoAnLTW.Web/Controllers/ReportsController.cs:99`: `SendWeeklyToEmail(...)`
- `DoAnLTW.Web/Controllers/ReportsController.cs:126`: `SendSummaryToEmail(...)`
- `DoAnLTW.Web/Services/Finance/ReportService.cs:17`: `BuildSummaryAsync(...)`
- `DoAnLTW.Web/Services/Finance/ReportService.cs:67`: `ExportExcelAsync(...)`
- `DoAnLTW.Web/Services/Finance/ReportService.cs:113`: `BuildWeeklyEmailAsync(...)`

**Hàng đợi email và worker nền**

- `DoAnLTW.Web/Services/Email/EmailQueue.cs:9`: `QueueAsync(...)`
- `DoAnLTW.Web/Services/Email/EmailQueue.cs:14`: `DequeueAsync(...)`
- `DoAnLTW.Web/Services/Email/QueuedEmailBackgroundService.cs:25`: dequeue mail
- `DoAnLTW.Web/Services/Email/QueuedEmailBackgroundService.cs:28`: gọi sender gửi mail
- `DoAnLTW.Web/Services/Email/SmtpEmailSender.cs:25`: `SendAsync(...)`
- `DoAnLTW.Web/Services/Email/SmtpEmailSender.cs:72`: `SaveDevelopmentCopyAsync(...)`

**Báo cáo tuần tự động**

- `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs:36`: `SendWeeklyReportsIfNeededAsync(...)`
- `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs:68-69`: build email và queue mail tuần

**Realtime**

- `DoAnLTW.Web/Hubs/BudgetHub.cs:7`: class `BudgetHub`
- `DoAnLTW.Web/Hubs/BudgetHub.cs:9`: `OnConnectedAsync()`
- `DoAnLTW.Web/Hubs/BudgetHub.cs:16`: add connection vào group `user-{userId}`
- `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:114`: server bắn event `budgetAlert`
- `DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs:37`: server bắn event `walletAlert`
- `DoAnLTW.Web/wwwroot/js/site.js:74`: client nhận `budgetAlert`
- `DoAnLTW.Web/wwwroot/js/site.js:79`: client nhận `walletAlert`

**Profile và admin**

- `DoAnLTW.Web/Controllers/ProfileController.cs:31`: load profile
- `DoAnLTW.Web/Controllers/ProfileController.cs:39`: update profile
- `DoAnLTW.Web/Services/Finance/AvatarStorageService.cs:12`: `SaveAsync(...)`
- `DoAnLTW.Web/Controllers/AdminController.cs:28`: `Dashboard(...)`
- `DoAnLTW.Web/Controllers/AdminController.cs:83`: `Users(...)`
- `DoAnLTW.Web/Controllers/AdminController.cs:148`: `Categories(...)`
- `DoAnLTW.Web/Controllers/AdminController.cs:202`: `Logs(...)`
- `DoAnLTW.Web/Controllers/AdminController.cs:240`: `ToggleUser(...)`
- `DoAnLTW.Web/Controllers/AdminController.cs:269`: `SaveCategory(...)`
- `DoAnLTW.Web/Controllers/AdminController.cs:349`: `DeleteCategory(...)`
- `DoAnLTW.Web/Services/Finance/AuditLogService.cs:15`: `WriteAsync(...)`

### 5.4. Các luồng gọi qua lại: request “ping” sang file nào

Đọc phần này như một sơ đồ mũi tên.

**Luồng đăng ký**

`AuthController.Register(...)`  
`DoAnLTW.Web/Controllers/AuthController.cs:93`  
-> `PasswordService.Hash(...)`  
`DoAnLTW.Web/Services/Security/PasswordService.cs:11`  
-> lưu `AppUser` vào `Users`  
-> `OtpService.SendOtpAsync(...)`  
`DoAnLTW.Web/Services/Auth/OtpService.cs:21`  
-> `EmailQueue.QueueAsync(...)`  
`DoAnLTW.Web/Services/Email/EmailQueue.cs:9`  
-> `QueuedEmailBackgroundService` lấy mail  
`DoAnLTW.Web/Services/Email/QueuedEmailBackgroundService.cs:25`  
-> `SmtpEmailSender.SendAsync(...)`  
`DoAnLTW.Web/Services/Email/SmtpEmailSender.cs:25`

**Luồng đăng nhập**

`AuthController.Login(...)`  
`DoAnLTW.Web/Controllers/AuthController.cs:49`  
-> query `Users`  
-> `PasswordService.Verify(...)`  
`DoAnLTW.Web/Services/Security/PasswordService.cs:18`  
-> `CookieAuthService.SignInAsync(...)`  
`DoAnLTW.Web/Services/Auth/CookieAuthService.cs:10`  
-> cookie được tạo  
-> các controller khác đọc `CurrentUserId`  
`DoAnLTW.Web/Controllers/AppControllerBase.cs:11`  
-> `ClaimsPrincipalExtensions.GetUserId(...)`  
`DoAnLTW.Web/Extensions/ClaimsPrincipalExtensions.cs:7`

**Luồng gợi ý danh mục**

Frontend gọi `TransactionsController.SuggestCategory(...)`  
`DoAnLTW.Web/Controllers/TransactionsController.cs:222`  
-> `CategorizationService.SuggestAsync(...)`  
`DoAnLTW.Web/Services/Finance/CategorizationService.cs:55`  
-> service normalize note  
`CategorizationService.cs:159`  
-> tách keyword  
`CategorizationService.cs:170`  
-> match `UserPersonalKeyword` trước  
-> nếu không có thì match `ExpenseRules/IncomeRules`  
-> trả JSON cho frontend

**Luồng lưu giao dịch**

`TransactionsController.Save(...)`  
`DoAnLTW.Web/Controllers/TransactionsController.cs:59`  
-> kiểm tra ví và category  
-> lưu `WalletTransaction`  
-> cập nhật `Wallet.CurrentBalance`  
-> nếu có chỉnh khác gợi ý thì `CategorizationService.LearnAsync(...)`  
`DoAnLTW.Web/Services/Finance/CategorizationService.cs:109`  
-> nếu là `Expense` thì `BudgetMonitorService.CheckAndNotifyAsync(...)`  
`DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:40`  
-> đồng thời gọi `WalletBalanceMonitorService.NotifyIfThresholdCrossedAsync(...)`  
`DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs:23`

**Luồng cảnh báo ngân sách**

`BudgetMonitorService.CheckAndNotifyAsync(...)`  
`DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:40`  
-> query `Budgets` và `Transactions`  
-> tính `spentAmount`, `usagePercent`  
-> tạo `BudgetAlert`  
-> SignalR server bắn `budgetAlert`  
`BudgetMonitorService.cs:114`  
-> `BudgetHub` đã group sẵn theo `user-{userId}`  
`DoAnLTW.Web/Hubs/BudgetHub.cs:16`  
-> client nhận ở `site.js`  
`DoAnLTW.Web/wwwroot/js/site.js:74`  
-> hiện toast realtime  
-> nếu đủ điều kiện thì queue email  
`BudgetMonitorService.cs:196`  
-> `EmailQueue.QueueAsync(...)`  
`EmailQueue.cs:9`

**Luồng cảnh báo ví xuống thấp**

`WalletBalanceMonitorService.NotifyIfThresholdCrossedAsync(...)`  
`DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs:23`  
-> nếu cắt ngưỡng thấp thì SignalR server bắn `walletAlert`  
`WalletBalanceMonitorService.cs:37`  
-> client nhận ở `site.js`  
`DoAnLTW.Web/wwwroot/js/site.js:79`

**Luồng dashboard**

`DashboardController.Index(...)`  
`DoAnLTW.Web/Controllers/DashboardController.cs:27`  
-> query `Wallets`, `Transactions`, `BudgetAlerts`  
-> build monthly comparison  
-> build category breakdown  
-> `BuildBudgetProgressAsync(...)`  
`DashboardController.cs:220`  
-> nếu là tháng hiện tại thì gọi `ForecastService.CalculateMonthEndExpenseForecastAsync(...)`  
`DoAnLTW.Web/Services/Finance/ForecastService.cs:15`  
-> trả `DashboardViewModel` cho view

**Luồng báo cáo**

`ReportsController.Index(...)`  
`DoAnLTW.Web/Controllers/ReportsController.cs:26`  
-> `ReportService.BuildSummaryAsync(...)`  
`DoAnLTW.Web/Services/Finance/ReportService.cs:17`  
-> render giao diện báo cáo

**Luồng export Excel**

`ReportsController.ExportExcel(...)`  
`DoAnLTW.Web/Controllers/ReportsController.cs:62`  
-> `ReportService.ExportExcelAsync(...)`  
`DoAnLTW.Web/Services/Finance/ReportService.cs:67`  
-> bên trong gọi lại `BuildSummaryAsync(...)`  
`ReportService.cs:69`  
-> tạo file `.xlsx` bằng ClosedXML  
-> trả file response cho trình duyệt

**Luồng gửi email báo cáo**

`ReportsController.SendSummaryToEmail(...)`  
`DoAnLTW.Web/Controllers/ReportsController.cs:126`  
hoặc `SendWeeklyToEmail(...)`  
`ReportsController.cs:99`  
-> `ReportService.BuildSummaryAsync(...)` hoặc `BuildWeeklyEmailAsync(...)`  
`ReportService.cs:17`, `ReportService.cs:113`  
-> tạo `EmailMessage`  
-> `EmailQueue.QueueAsync(...)`  
`EmailQueue.cs:9`  
-> worker dequeue và gửi thật  
`QueuedEmailBackgroundService.cs:25,28`

**Luồng báo cáo tuần tự động**

`WeeklyReportHostedService.SendWeeklyReportsIfNeededAsync(...)`  
`DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs:36`  
-> duyệt user đủ điều kiện  
-> `ReportService.BuildWeeklyEmailAsync(...)`  
`ReportService.cs:113`  
-> `EmailQueue.QueueAsync(...)`  
`WeeklyReportHostedService.cs:69`  
-> ghi `ReportDispatchLog`

**Luồng profile**

`ProfileController.Index(...)`  
`DoAnLTW.Web/Controllers/ProfileController.cs:39`  
-> `AvatarStorageService.SaveAsync(...)`  
`DoAnLTW.Web/Services/Finance/AvatarStorageService.cs:12`  
-> cập nhật `Users`  
-> nếu đổi mật khẩu thì dùng `PasswordService.Hash(...)`

## 6. Luồng khởi động ứng dụng

1. `Program.cs` đọc config
2. kiểm tra `DefaultConnection`
3. bật MVC, SignalR, Auth, Data Protection
4. đăng ký `FinanceDbContext`
5. đăng ký service và background worker
6. chạy `SeedData.InitializeAsync(...)`
7. map route MVC và `BudgetHub`
8. app bắt đầu nhận request

## 7. Luồng chức năng chính

### 7.1. Luồng đăng ký tài khoản

Nguồn đọc:

- `DoAnLTW.Web/Controllers/AuthController.cs`
- `DoAnLTW.Web/Services/Security/PasswordService.cs`
- `DoAnLTW.Web/Services/Auth/OtpService.cs`

Luồng:

1. User nhập username, display name, email, password
2. `AuthController.Register(...)` kiểm tra trùng username và email
3. `PasswordService.HashPassword(...)` băm mật khẩu
4. tạo `AppUser` với role `User`
5. `OtpService.SendOtpAsync(...)` sinh OTP, lưu `EmailOtp`, gửi mail
6. redirect sang màn hình nhập OTP

### 7.2. Luồng xác thực OTP

1. User nhập mã
2. `AuthController.VerifyOtp(...)` gọi `OtpService.VerifyAsync(...)`
3. service lấy OTP mới nhất chưa dùng
4. so hash OTP
5. kiểm tra hết hạn, số lần sai, trạng thái `IsUsed`
6. nếu thành công:
   - đánh dấu `IsEmailVerified = true`
   - có thể cho đăng nhập hoặc chuyển sang bước kế tiếp tùy purpose

### 7.3. Luồng đăng nhập

Nguồn đọc:

- `AuthController.Login(...)`
- `CookieAuthService.SignInAsync(...)`

Luồng:

1. tìm user theo email
2. kiểm tra `IsActive`
3. kiểm tra `IsEmailVerified`
4. `PasswordService.Verify(...)`
5. `CookieAuthService.SignInAsync(...)` tạo cookie claim
6. redirect về dashboard theo role

### 7.4. Luồng tạo và cập nhật ví

Nguồn đọc:

- `WalletsController.cs`

Luồng:

1. load danh sách ví ở `Index(...)`
2. `Save(...)` tạo mới hoặc cập nhật
3. khi sửa `InitialBalance`, controller tính `delta`
4. cộng `delta` vào `CurrentBalance`
5. nếu ví đã có giao dịch thì không cho xóa

### 7.5. Luồng thêm giao dịch

Nguồn đọc:

- `TransactionsController.cs`
- `CategorizationService.cs`
- `BudgetMonitorService.cs`
- `WalletBalanceMonitorService.cs`

Luồng:

1. user nhập loại giao dịch, ví, danh mục, số tiền, note
2. controller kiểm tra ví có thuộc user không
3. controller kiểm tra category có hợp lệ với loại giao dịch không
4. nếu cần, frontend gọi `SuggestCategory(...)`
5. `CategorizationService.SuggestAsync(...)` gợi ý category theo note
6. lưu `WalletTransaction`
7. cập nhật `Wallet.CurrentBalance`
8. nếu user sửa category so với gợi ý, gọi `CategorizationService.LearnAsync(...)`
9. nếu là `Expense`, gọi `BudgetMonitorService.CheckAndNotifyAsync(...)`
10. gọi `WalletBalanceMonitorService.NotifyIfThresholdCrossedAsync(...)`

### 7.6. Luồng sửa giao dịch

Nguồn đọc:

- `TransactionsController.Save(...)`

Điểm quan trọng:

1. lấy transaction cũ
2. `ReverseWalletBalance(...)` để hoàn tác dữ liệu cũ
3. cập nhật transaction mới
4. `ApplyWalletBalance(...)` với dữ liệu mới
5. xóa các `BudgetAlert` cũ gắn với transaction này nếu cần
6. tính lại budget alert
7. kiểm tra cảnh báo số dư thấp

### 7.7. Luồng xóa giao dịch

1. tìm transaction theo `id` và `CurrentUserId`
2. hoàn tác ảnh hưởng lên ví
3. xóa `BudgetAlert` liên quan
4. xóa transaction
5. lưu DB

### 7.8. Luồng ngân sách và cảnh báo

Nguồn đọc:

- `BudgetsController.cs`
- `BudgetMonitorService.cs`

Luồng:

1. user tạo budget theo category, month, year
2. controller kiểm tra không trùng `(UserId, CategoryId, Year, Month)`
3. sau khi phát sinh `Expense`, service tính `spentAmount`
4. `usagePercent = spentAmount / LimitAmount * 100`
5. nếu `usagePercent >= 100` thì tạo `BudgetAlert`
6. lưu alert vào DB
7. bắn SignalR event `budgetAlert`
8. thử gửi email cảnh báo

### 7.9. Luồng dashboard

Nguồn đọc:

- `DashboardController.cs`
- `Views/Dashboard/Index.cshtml`

Dashboard tổng hợp:

- tổng số dư ví
- tổng thu tháng
- tổng chi tháng
- tỷ lệ tiết kiệm
- chi tiêu 7 ngày gần nhất
- so sánh 12 tháng
- breakdown category
- budget progress
- cảnh báo gần đây
- giao dịch gần đây
- dự báo chi tiêu cuối tháng

### 7.10. Luồng báo cáo

Nguồn đọc:

- `ReportsController.cs`
- `ReportService.cs`

Luồng:

1. user chọn khoảng ngày
2. `ReportsController.Index(...)` gọi `ReportService.BuildSummaryAsync(...)`
3. service tính tổng thu, tổng chi, chênh lệch, transaction count, average expense/day, top categories
4. user có thể:
   - export Excel
   - gửi email summary
   - gửi weekly report

### 7.11. Luồng gửi email

Nguồn đọc:

- `EmailQueue.cs`
- `QueuedEmailBackgroundService.cs`
- `SmtpEmailSender.cs`

Luồng:

1. controller hoặc service tạo `EmailMessage`
2. message được `QueueAsync(...)` vào `EmailQueue`
3. background worker dequeue
4. resolve `IEmailSender`
5. `SmtpEmailSender.SendAsync(...)`
6. nếu thiếu SMTP config trong môi trường dev thì lưu bản HTML local

### 7.12. Luồng báo cáo tuần tự động

Nguồn đọc:

- `WeeklyReportHostedService.cs`
- `ReportService.cs`
- `ReportDispatchLog`

Luồng:

1. worker chạy nền theo chu kỳ
2. kiểm tra có phải đúng thời điểm gửi hay không
3. lấy user đủ điều kiện
4. build report tuần
5. queue email
6. ghi `ReportDispatchLog`
7. tránh gửi trùng cùng `PeriodKey`

### 7.13. Luồng admin

Nguồn đọc:

- `AdminController.cs`

Admin có thể:

- xem dashboard quản trị
- xem danh sách user
- khóa, mở khóa user
- xem category
- tạo, sửa, xóa category
- xem logs

Ràng buộc quan trọng:

- không khóa admin khác bừa bãi
- không đổi `TransactionType` của category nếu đã có transaction hoặc budget
- không xóa category nếu đã có ràng buộc nghiệp vụ

## 8. Thuật toán và công thức quan trọng

### 8.1. Mật khẩu

Nguồn đọc:

- `PasswordService.cs`

Định dạng hash:

`v1.iterations.salt.hash`

Ý nghĩa:

- `v1`: version thuật toán
- `iterations`: số vòng PBKDF2
- `salt`: muối
- `hash`: kết quả cuối

### 8.2. OTP

Nguồn đọc:

- `OtpService.cs`

Điểm chính:

- OTP là mã 6 số
- lưu `CodeHash`, không lưu mã gốc
- có `ExpiresAt`
- có `Attempts`
- chỉ dùng một lần

### 8.3. Công thức số dư ví

- `Expense`: `CurrentBalance = CurrentBalance - Amount`
- `Income`: `CurrentBalance = CurrentBalance + Amount`

### 8.4. Công thức budget

- `usagePercent = spentAmount / limitAmount * 100`

Phiên bản hiện tại:

- chỉ cảnh báo khi `usagePercent >= 100`

### 8.5. Công thức dashboard

- `NetAmount = Income - Expense`
- `SavingsRate = (TotalIncomeMonth - TotalExpenseMonth) / TotalIncomeMonth * 100`

### 8.6. Gợi ý danh mục

Nguồn đọc:

- `CategorizationService.cs`

Ý tưởng:

- normalize note
- tách keyword
- ưu tiên `UserPersonalKeyword`
- nếu không match thì dùng `ExpenseRules` hoặc `IncomeRules`
- nếu user sửa gợi ý thì hệ thống học lại

### 8.7. Dự báo chi tiêu cuối tháng

Nguồn đọc:

- `ForecastService.cs`

Các công thức quan trọng:

`T = max(Q3 + 1.5 * IQR, 3 * Median, 500000)`

`ForecastCurrent = SpikeTotal + (RoutineTotal / d) * D`

`Forecast = 0.65 * ForecastCurrent + 0.35 * Avg3`

Ý nghĩa:

- tách chi tiêu đột biến ra khỏi chi tiêu thường xuyên
- nội suy phần chi thường xuyên cho cả tháng
- trộn dữ liệu tháng hiện tại với lịch sử 3 tháng gần nhất

Lưu ý bản vá hiện tại:

- với tài khoản mới tạo giữa tháng, phần nội suy dùng khoảng quan sát từ ngày tạo tài khoản để tránh dự báo bị phóng đại trong tháng đầu

### 8.8. Giải thích sâu về bảo mật: hash, mã hóa, bất đối xứng

Nguồn đọc:

- `DoAnLTW.Web/Services/Security/PasswordService.cs`
- `DoAnLTW.Web/Program.cs`

Điểm rất quan trọng:

- Dự án **không dùng mã hóa bất đối xứng** cho mật khẩu.
- Dự án cũng **không mã hóa đối xứng** mật khẩu.
- Dự án dùng **hash một chiều** bằng PBKDF2 + SHA256 + salt.

Vì sao?

- Với mật khẩu, mục tiêu không phải “giải mã lại” mà là “so sánh xem người dùng nhập đúng hay không”.
- Nếu dùng mã hóa đối xứng, server phải giữ khóa giải mã. Nếu khóa bị lộ thì toàn bộ password có thể bị giải ra.
- Nếu dùng bất đối xứng, bài toán cũng không phù hợp vì password không cần giải mã bằng public/private key như ký số hay trao đổi khóa.
- Cách đúng cho password là hash một chiều, có salt, có nhiều vòng lặp để làm chậm brute force.

Trong `PasswordService.cs`:

- `SaltSize = 16` ở dòng 7
- `HashSize = 32` ở dòng 8
- `Iterations = 100_000` ở dòng 9
- `Hash(...)` ở dòng 11
- `Verify(...)` ở dòng 18

Giải thích từng thông số:

- `SaltSize = 16` byte:
  - đủ để tạo salt ngẫu nhiên mạnh
  - giúp hai user cùng password vẫn cho ra hai hash khác nhau
  - chống precomputed rainbow table tốt hơn

- `HashSize = 32` byte:
  - tương ứng 256 bit
  - cân bằng giữa độ mạnh và kích thước lưu trữ
  - phù hợp với `SHA256`

- `Iterations = 100000`:
  - làm việc băm chậm đi có chủ đích
  - nếu DB bị lộ, attacker sẽ khó brute force nhanh trên số lượng lớn password hơn
  - đây là mức tương đối hợp lý cho đồ án web, đủ để giải thích được ý tưởng “key stretching”

- `HashAlgorithmName.SHA256` ở dòng 14 và 34:
  - là hàm băm nền bên trong PBKDF2
  - mạnh hơn các cấu hình cũ như SHA1

`CryptographicOperations.FixedTimeEquals(...)` ở dòng 36:

- dùng để so sánh hash theo thời gian hằng
- giảm rủi ro timing attack

**Data Protection có vai trò gì?**

Trong `Program.cs`:

- `AddDataProtection()` ở dòng 39
- `PersistKeysToFileSystem(...)` ở dòng 41

Data Protection không dùng để băm password. Nó chủ yếu giúp ASP.NET Core bảo vệ dữ liệu nội bộ như cookie xác thực. Nói ngắn gọn:

- password -> do `PasswordService` xử lý bằng PBKDF2
- cookie auth -> do framework + Data Protection bảo vệ

### 8.9. Cookie Authentication hoạt động ra sao

Nguồn đọc:

- `DoAnLTW.Web/Program.cs`
- `DoAnLTW.Web/Services/Auth/CookieAuthService.cs`
- `DoAnLTW.Web/Extensions/ClaimsPrincipalExtensions.cs`
- `DoAnLTW.Web/Controllers/AppControllerBase.cs`

Nơi cấu hình cookie:

- `Program.cs:49-56`

Các option đang dùng:

- `LoginPath = /Auth/Login` ở dòng 52
- `AccessDeniedPath = /Auth/AccessDenied` ở dòng 53
- `SlidingExpiration = true` ở dòng 54
- `ExpireTimeSpan = 14 ngày` ở dòng 55

Luồng cookie auth:

1. User gửi form login đến `AuthController.Login(...)`
2. Controller kiểm tra email, trạng thái tài khoản, `IsEmailVerified`
3. `PasswordService.Verify(...)` xác minh mật khẩu
4. Nếu đúng, controller gọi `CookieAuthService.SignInAsync(...)`
5. Trong `CookieAuthService.cs:12-19`, service tạo claims:
   - `ClaimTypes.NameIdentifier`
   - `ClaimTypes.Name`
   - `ClaimTypes.GivenName`
   - `ClaimTypes.Email`
   - `ClaimTypes.Role`
6. Tại `CookieAuthService.cs:24-31`, framework tạo cookie và trả về cho trình duyệt
7. Ở request sau, middleware `UseAuthentication()` tại `Program.cs:89` đọc cookie này
8. User identity được gắn vào `HttpContext.User`
9. Các controller lấy `CurrentUserId` qua:
   - `AppControllerBase.cs:11`
   - `ClaimsPrincipalExtensions.cs:7`

Vì sao dùng cookie auth hợp với đồ án này?

- đây là web app MVC render server-side
- cùng domain, cùng server
- không cần phức tạp hóa bằng JWT
- phù hợp đúng kiến thức Lập trình Web truyền thống

### 8.10. Dependency Injection đang tiêm ở đâu và hoạt động thế nào

Nguồn đọc:

- `DoAnLTW.Web/Program.cs`
- các constructor của controller và service

Nơi đăng ký DI:

- `Program.cs:60-73`

Ví dụ đăng ký:

- `PasswordService` ở dòng 60
- `CookieAuthService` ở dòng 61
- `OtpService` ở dòng 62
- `CategorizationService` ở dòng 63
- `ForecastService` ở dòng 64
- `BudgetMonitorService` ở dòng 65
- `WalletBalanceMonitorService` ở dòng 66
- `ReportService` ở dòng 67
- `AuditLogService` ở dòng 68
- `AvatarStorageService` ở dòng 69
- `IEmailSender -> SmtpEmailSender` ở dòng 70
- `EmailQueue` là `Singleton` ở dòng 71
- `QueuedEmailBackgroundService` ở dòng 72
- `WeeklyReportHostedService` ở dòng 73

DI được “tiêm” vào constructor như thế nào?

Ví dụ:

- `AuthController` nhận service ở `AuthController.cs:21`
- `TransactionsController` nhận service ở `TransactionsController.cs:20`
- `DashboardController` nhận service ở `DashboardController.cs:17`
- `OtpService` nhận `FinanceDbContext` và `EmailQueue` ở `OtpService.cs:15`
- `BudgetMonitorService` nhận `FinanceDbContext`, `IHubContext<BudgetHub>`, `EmailQueue` ở `BudgetMonitorService.cs:33`
- `QueuedEmailBackgroundService` nhận `EmailQueue`, `IServiceProvider`, `ILogger` ở `QueuedEmailBackgroundService.cs:9`

Giải thích vòng đời:

- `Scoped`:
  - sống trong phạm vi một request
  - hợp với service dùng `FinanceDbContext`
  - ví dụ `ForecastService`, `ReportService`, `OtpService`

- `Singleton`:
  - sống suốt vòng đời app
  - hợp với đối tượng dùng chung
  - ở đây là `EmailQueue`

- `HostedService`:
  - không chạy theo request
  - chạy nền cùng vòng đời app
  - ví dụ gửi email nền và báo cáo tuần

Vì sao `QueuedEmailBackgroundService` phải tạo scope mới?

Trong `QueuedEmailBackgroundService.cs`:

- dequeue mail ở dòng 25
- `CreateScope()` ở dòng 26
- resolve `IEmailSender` ở dòng 27

Lý do:

- background service sống lâu, còn nhiều dependency bên trong là `Scoped`
- nếu không tạo scope mới thì rất dễ sai vòng đời object
- đây là cách chuẩn khi worker nền cần dùng service phụ thuộc vào request scope hoặc `DbContext`

### 8.11. OTP và các thông số bảo mật của OTP

Nguồn đọc:

- `DoAnLTW.Web/Services/Auth/OtpService.cs`

Thông số hiện tại:

- `RandomNumberGenerator.GetInt32(100000, 999999)` ở dòng 23
- `ExpiresAt = DateTime.UtcNow.AddMinutes(10)` ở dòng 31
- giới hạn sai quá 5 lần ở dòng 77-83
- hash OTP bằng `SHA256` ở dòng 96-99

Giải thích:

- mã 6 chữ số:
  - dễ nhập với người dùng
  - phù hợp với email OTP trong đồ án

- hiệu lực 10 phút:
  - đủ thời gian cho người dùng mở email và nhập
  - không quá dài để giảm rủi ro bị lợi dụng

- quá 5 lần thì chặn:
  - chống brute force
  - vì không gian tìm kiếm OTP nhỏ hơn password nên phải siết số lần thử

- OTP cũng hash thay vì lưu plain text:
  - nếu DB lộ thì attacker không đọc được mã trực tiếp

Lưu ý kỹ thuật:

- hàm `GetInt32(min, max)` dùng upper bound kiểu exclusive
- nghĩa là `999999` hiện tại không được sinh ra
- đây không phải lỗi nghiêm trọng về bảo mật, nhưng nếu muốn chặt thì có thể đổi thành `GetInt32(100000, 1000000)`

## 9. File nào nên mở khi bị hỏi một chức năng

- hỏi đăng ký, login, OTP: `AuthController.cs`, `OtpService.cs`, `PasswordService.cs`, `CookieAuthService.cs`
- hỏi database: `FinanceDbContext.cs`, `IdentityEntities.cs`, `FinanceEntities.cs`, `SystemEntities.cs`
- hỏi ví: `WalletsController.cs`
- hỏi giao dịch: `TransactionsController.cs`
- hỏi gợi ý danh mục: `CategorizationService.cs`
- hỏi ngân sách và cảnh báo: `BudgetsController.cs`, `BudgetMonitorService.cs`, `WalletBalanceMonitorService.cs`
- hỏi dashboard: `DashboardController.cs`, `Views/Dashboard/Index.cshtml`
- hỏi dự báo: `ForecastService.cs`
- hỏi báo cáo và Excel: `ReportsController.cs`, `ReportService.cs`
- hỏi email nền: `EmailQueue.cs`, `QueuedEmailBackgroundService.cs`, `SmtpEmailSender.cs`
- hỏi báo cáo tuần: `WeeklyReportHostedService.cs`, `ReportDispatchLog`
- hỏi admin: `AdminController.cs`

## 10. Thứ tự đọc code nếu muốn hiểu sâu

### 10.1. Đọc theo luồng hệ thống

1. `Program.cs`
2. `FinanceDbContext.cs`
3. `SeedData.cs`
4. `AuthController.cs`
5. `TransactionsController.cs`
6. `BudgetMonitorService.cs`
7. `DashboardController.cs`
8. `ForecastService.cs`
9. `ReportsController.cs`
10. `AdminController.cs`

### 10.2. Đọc theo luồng người dùng

1. đăng ký, OTP, login
2. tạo ví
3. tạo giao dịch
4. xem dashboard
5. tạo ngân sách
6. phát sinh cảnh báo
7. xem báo cáo, export Excel, gửi email

## 11. Bộ câu hỏi vấn đáp kiểu giáo viên môn Lập trình Web

Các câu dưới đây đều bám trực tiếp vào đồ án. Khi học, hãy mở file tương ứng và tự trả lời theo code.

### 11.1. Câu hỏi về kiến trúc và khởi động

1. Vì sao dự án chọn ASP.NET Core MVC thay vì SPA thuần?
2. `Program.cs` đang đóng vai trò gì trong toàn hệ thống?
3. Vì sao phải gọi `UseAuthentication()` trước `UseAuthorization()`?
4. Vì sao `EmailQueue` là `Singleton` nhưng `ForecastService` lại là `Scoped`?
5. Vì sao `QueuedEmailBackgroundService` và `WeeklyReportHostedService` phải đăng ký bằng `AddHostedService(...)`?
6. `SeedData.InitializeAsync(...)` đang chạy ở đâu trong vòng đời ứng dụng?
7. Vì sao app ném lỗi ngay nếu thiếu `DefaultConnection`?

### 11.2. Câu hỏi về database

8. Vì sao `AppUser.Email` và `AppUser.Username` phải unique?
9. Vì sao `Budget(UserId, CategoryId, Year, Month)` phải unique?
10. Nếu bỏ unique index của budget thì lỗi nghiệp vụ nào xảy ra?
11. Vì sao `WalletTransaction -> Wallet` dùng `Restrict`?
12. Vì sao `SystemLog -> User` dùng `SetNull`?
13. Vì sao `BudgetAlert -> User/Budget` dùng `NoAction`?
14. Vì sao tiền nên lưu bằng `decimal(18,2)` thay vì `double`?
15. Bảng nào là trung tâm nhất của nghiệp vụ tài chính?

### 11.3. Câu hỏi về xác thực và bảo mật

16. Password được lưu theo cơ chế nào?
17. Chuỗi hash `v1.iterations.salt.hash` có ý nghĩa gì?
18. Tại sao OTP không lưu mã gốc mà chỉ lưu hash?
19. Tại sao OTP có `Attempts`?
20. Cookie login đang chứa những claim nào?
21. Vì sao hệ thống không cho đăng nhập nếu `IsEmailVerified = false`?
22. `FixedTimeEquals(...)` chống kiểu tấn công gì?

### 11.4. Câu hỏi về ví và giao dịch

23. Luồng thêm giao dịch mới chạy qua những file nào?
24. Vì sao khi sửa giao dịch phải reverse dữ liệu cũ trước?
25. Nếu đổi giao dịch từ ví A sang ví B thì vì sao phải theo dõi cả 2 ví?
26. Tại sao xóa giao dịch phải xóa các `BudgetAlert` liên quan?
27. Tại sao không cho xóa ví đã có giao dịch?
28. Công thức cập nhật số dư ví là gì?
29. Nếu category tồn tại nhưng `TransactionType` không khớp thì controller xử lý thế nào?

### 11.5. Câu hỏi về gợi ý danh mục

30. `CategorizationService` là AI hay heuristic?
31. Vì sao phải normalize tiếng Việt và bỏ dấu?
32. Vì sao ưu tiên `UserPersonalKeyword` trước `ExpenseRules` và `IncomeRules`?
33. Vì sao chỉ học lại khi user sửa gợi ý?
34. Vì sao chỉ lấy một số keyword chính thay vì học toàn bộ note?

### 11.6. Câu hỏi về ngân sách và cảnh báo

35. Luồng kiểm tra vượt ngân sách chạy ở đâu?
36. Vì sao chỉ check budget cho `Expense`?
37. Công thức `usagePercent` là gì?
38. Vì sao phiên bản hiện tại chỉ cảnh báo khi `>= 100%`?
39. Vì sao phải lưu `BudgetAlert` vào DB dù đã có SignalR?
40. Vì sao phải có `IsRead` và `IsEmailSent`?
41. Cơ chế chống gửi email cảnh báo trùng đang làm như thế nào?

### 11.7. Câu hỏi về dashboard và dự báo

42. Dashboard đang tổng hợp những chỉ số gì?
43. Vì sao `SavingsRate` cần chặn chia cho 0?
44. Dự báo chi tiêu tháng cuối được tính theo thuật toán nào?
45. `Q1`, `Q3`, `IQR`, `Median` là gì?
46. Vì sao lại có số `1.5` trong công thức phát hiện outlier?
47. Vì sao lại có mốc `500000`?
48. `0.65` và `0.35` có ý nghĩa gì?
49. Vì sao tài khoản mới tạo giữa tháng từng dễ bị dự báo sai?
50. Bản vá hiện tại sửa lỗi đó như thế nào?

### 11.8. Câu hỏi về báo cáo, Excel, email

51. `ReportService.BuildSummaryAsync(...)` đang tính những gì?
52. Vì sao dùng `endExclusive = to + 1 ngày` thay vì `<= to`?
53. Vì sao `AverageExpensePerDay` tính theo số ngày của khoảng báo cáo?
54. ClosedXML đóng vai trò gì trong đồ án?
55. Vì sao không gửi email trực tiếp trong request?
56. `EmailQueue` hoạt động như thế nào?
57. Nếu app restart giữa chừng thì email trong queue sẽ ra sao?
58. Vì sao `ReportDispatchLog` cần unique theo `(UserId, ReportType, PeriodKey)`?

### 11.9. Câu hỏi về admin và vận hành

59. Admin dashboard hiển thị những gì?
60. Vì sao không cho đổi `TransactionType` của category nếu category đã có transaction hoặc budget?
61. Vì sao không cho xóa category nếu đã có ràng buộc nghiệp vụ?
62. `SystemLog` dùng để làm gì ngoài việc “ghi log cho có”?
63. Nếu dữ liệu lớn lên, phần nào trong hệ thống cần tối ưu trước?

## 12. Bộ câu hỏi khó kiểu giảng viên xoáy sâu vào đồ án

1. Vì sao `ForecastService` không lấy trung bình cộng đơn giản?
2. Nếu tháng hiện tại chưa có giao dịch thì vì sao dùng `Avg3`?
3. Nếu tháng hiện tại có 1 khoản chi cực lớn đầu tháng thì thuật toán hiện tại phản ứng ra sao?
4. Nếu user mới tạo tài khoản ngày 25 thì việc nội suy tháng có rủi ro gì?
5. Vì sao `BudgetMonitorService` vừa lưu alert vào DB vừa bắn SignalR?
6. Nếu SignalR fail nhưng DB save thành công thì hệ thống có chấp nhận được không?
7. Nếu DB save fail mà UI đã thấy toast thì có inconsistency không?
8. Tại sao `WalletBalanceMonitorService` chỉ cảnh báo khi vừa cắt ngưỡng thấp?
9. Vì sao background service phải tạo scope DI mới khi gửi mail?
10. Tại sao `SmtpEmailSender` có fallback lưu file HTML ở môi trường dev?
11. Nếu deploy nhiều instance app thì `EmailQueue` trong memory có còn phù hợp không?
12. Nếu deploy nhiều instance, SignalR cần thêm gì để scale?
13. Nếu thầy cô hỏi “điểm thông minh thật sự của đồ án là gì” thì em trả lời thế nào?
14. Nếu thầy cô hỏi “điểm yếu kỹ thuật lớn nhất của đồ án là gì” thì em tự nhận phần nào?
15. Nếu làm lại từ đầu, em sẽ refactor mạnh nhất controller nào? Vì sao?

## 13. Bộ câu hỏi đi thẳng vào file code

Hãy mở file tương ứng rồi tự giải thích:

- `Program.cs`: giải thích toàn bộ pipeline khởi động
- `FinanceDbContext.cs`: giải thích từng index, từng delete behavior
- `AuthController.cs`: giải thích luồng register, verify OTP, login, reset password
- `TransactionsController.cs`: giải thích create, update, delete transaction
- `CategorizationService.cs`: giải thích vì sao nói đây là heuristic chứ không phải ML
- `BudgetMonitorService.cs`: giải thích vì sao alert hiện trên màn hình và có thể gửi email
- `ForecastService.cs`: giải thích từng công thức và từng con số
- `ReportService.cs`: giải thích cách tổng hợp báo cáo và export Excel
- `WeeklyReportHostedService.cs`: giải thích job báo cáo tuần
- `AdminController.cs`: giải thích vì sao module admin cần nhiều ràng buộc nghiệp vụ

## 14. Cách trả lời khi bị hỏi xoáy

Khi giảng viên hỏi, nên trả lời theo 4 bước:

1. Nói mục tiêu nghiệp vụ
2. Chỉ ra file code xử lý
3. Nói luồng chính hoặc công thức
4. Nói lý do thiết kế hoặc rủi ro nếu làm cách khác

Ví dụ:

- “Phần này nằm ở `ForecastService.cs`”
- “Luồng là lấy expense tháng hiện tại, tách spike, nội suy routine, rồi trộn với `Avg3`”
- “Lý do không dùng trung bình cộng đơn giản là vì sẽ bị lệch mạnh khi có khoản chi đột biến”

## 15. Kết luận để học thuộc nhanh

Nếu phải tóm dự án trong 30 giây:

Đây là một ứng dụng ASP.NET Core MVC quản lý chi tiêu cá nhân, dùng SQL Server và EF Core để lưu dữ liệu, Cookie Authentication để đăng nhập, SignalR để cảnh báo realtime, BackgroundService và Channel để gửi email nền, ClosedXML để xuất Excel. Luồng nghiệp vụ quan trọng nhất là giao dịch vì nó kéo theo cập nhật số dư ví, kiểm tra ngân sách, sinh cảnh báo, cập nhật dashboard và báo cáo. Phần đáng nói nhất khi bảo vệ là database design, luồng transaction, gợi ý danh mục, dự báo chi tiêu và cơ chế email nền.

## 16. Câu hỏi và đáp án giải thích kỹ

Phần này được viết theo kiểu giảng viên hỏi và sinh viên trả lời. Khi học, bạn nên vừa đọc đáp án vừa mở file code tương ứng.

### 16.1. Kiến trúc, startup, dependency injection

**Câu 1. Vì sao dự án chọn ASP.NET Core MVC thay vì SPA thuần?**

Đáp án:
Dự án này có nhiều màn hình CRUD, form nhập liệu, phân quyền theo vai trò, báo cáo và giao diện quản trị. Với phạm vi của một đồ án môn Lập trình Web, ASP.NET Core MVC phù hợp vì cho phép tổ chức rõ ràng theo Model, View, Controller, dễ đi từ request đến action rồi render ra view. Nhóm cũng không cần thêm một lớp frontend riêng như React hoặc Angular, nên chi phí phát triển thấp hơn và dễ bám sát kiến thức môn học hơn. Ngoài ra, Razor View giúp hiển thị dữ liệu động ngay từ server, rất hợp với các trang như dashboard, báo cáo, ngân sách và admin.

**Câu 2. `Program.cs` đóng vai trò gì trong hệ thống?**

Đáp án:
`Program.cs` là điểm khởi động trung tâm của toàn bộ ứng dụng. File này chịu trách nhiệm đọc cấu hình, đăng ký `DbContext`, bật MVC, bật Cookie Authentication, bật SignalR, đăng ký toàn bộ service và worker nền, sau đó cấu hình middleware pipeline trước khi app nhận request. Khi giảng viên hỏi “hệ thống bắt đầu từ đâu”, câu trả lời đúng nhất là bắt đầu từ `Program.cs`, vì mọi khối quan trọng đều được nối lại ở đây.

**Câu 3. Vì sao phải gọi `UseAuthentication()` trước `UseAuthorization()`?**

Đáp án:
`UseAuthentication()` có nhiệm vụ đọc cookie hoặc thông tin xác thực để nhận diện người dùng hiện tại là ai. Sau khi người dùng đã được nhận diện, `UseAuthorization()` mới có dữ liệu để kiểm tra xem người đó có đủ quyền truy cập route hay không. Nếu đảo thứ tự, phần authorization sẽ chạy khi hệ thống chưa biết user là ai, dẫn đến việc kiểm tra quyền bị sai hoặc không hoạt động đúng.

**Câu 4. Vì sao `EmailQueue` là `Singleton` nhưng nhiều service khác là `Scoped`?**

Đáp án:
`EmailQueue` là một hàng đợi dùng chung cho toàn ứng dụng. Nếu mỗi request có một queue riêng thì email được enqueue ở request này sẽ không được worker của request khác nhìn thấy, nên queue phải là `Singleton`. Ngược lại, các service như `ForecastService`, `ReportService`, `BudgetMonitorService` hay `CategorizationService` thường làm việc cùng `FinanceDbContext`, mà `DbContext` phù hợp với vòng đời `Scoped`, tức sống trong phạm vi một request để tránh chia sẻ trạng thái dữ liệu giữa các request.

**Câu 5. Vì sao phải dùng `HostedService` cho gửi mail nền và báo cáo tuần?**

Đáp án:
Hai tác vụ này không nên gắn trực tiếp vào vòng đời của request web. Với gửi mail nền, người dùng chỉ cần bấm gửi là xong, còn việc kết nối SMTP và gửi mail có thể diễn ra sau ở nền để giao diện phản hồi nhanh hơn. Với báo cáo tuần, đây là một công việc định kỳ không phụ thuộc người dùng đang mở trang nào, nên phải có một background worker chạy độc lập trong ứng dụng. `HostedService` là cơ chế chuẩn của ASP.NET Core để giải quyết hai bài toán đó.

### 16.2. Database, quan hệ, ràng buộc

**Câu 6. Vì sao `AppUser.Email` và `AppUser.Username` phải unique?**

Đáp án:
Về nghiệp vụ, email và username là hai định danh quan trọng của người dùng. Nếu hai tài khoản trùng email hoặc trùng username thì quá trình đăng nhập, khôi phục mật khẩu, phân biệt chủ sở hữu dữ liệu và gửi OTP sẽ bị mơ hồ. Vì vậy ràng buộc unique ở database là lớp bảo vệ cuối cùng để đảm bảo tính toàn vẹn dữ liệu, kể cả khi controller hoặc UI lỡ kiểm tra thiếu.

**Câu 7. Vì sao `Budget(UserId, CategoryId, Year, Month)` phải unique?**

Đáp án:
Mỗi người dùng trong một tháng chỉ nên có một ngân sách cho một danh mục. Nếu cho phép trùng, hệ thống sẽ không biết phải so giao dịch với ngân sách nào, phần trăm sử dụng ngân sách sẽ bị lệch và dashboard sẽ có thể hiển thị hai budget cho cùng một mục. Vì vậy unique index ở đây vừa là ràng buộc kỹ thuật, vừa là ràng buộc nghiệp vụ.

**Câu 8. Vì sao `WalletTransaction -> Wallet` dùng `Restrict`?**

Đáp án:
`WalletTransaction` là dữ liệu lịch sử tài chính rất quan trọng. Nếu dùng `Cascade`, khi xóa ví có thể làm mất toàn bộ giao dịch gắn với ví đó, kéo theo sai báo cáo, dashboard và log tài chính. Dùng `Restrict` buộc hệ thống không được xóa ví nếu vẫn còn giao dịch tham chiếu, từ đó bảo vệ dữ liệu lịch sử.

**Câu 9. Vì sao `SystemLog -> User` dùng `SetNull`?**

Đáp án:
Log hệ thống cần được giữ lại để kiểm tra và truy vết, ngay cả khi user bị xóa hoặc không còn hoạt động. Nếu dùng `Cascade`, xóa user sẽ làm mất toàn bộ log liên quan. Dùng `SetNull` giúp giữ nguyên bản ghi log, chỉ bỏ liên kết user nếu cần.

**Câu 10. Vì sao tiền được lưu bằng `decimal(18,2)`?**

Đáp án:
Tiền là dữ liệu cần độ chính xác thập phân cao. `double` và `float` dùng biểu diễn số thực nhị phân nên dễ xuất hiện sai số rất nhỏ khi cộng trừ nhiều lần, ví dụ 0.1 + 0.2 không chắc ra đúng 0.3 theo nghĩa tuyệt đối. `decimal` phù hợp hơn cho dữ liệu tài chính vì được thiết kế để biểu diễn chính xác các số thập phân thường dùng trong tiền tệ.

### 16.3. Xác thực, OTP, cookie, bảo mật

**Câu 11. Password đang được lưu theo cơ chế nào?**

Đáp án:
Hệ thống không lưu mật khẩu thô mà lưu dưới dạng PBKDF2 có salt và số vòng lặp. Trong `PasswordService`, chuỗi hash được lưu theo dạng `v1.iterations.salt.hash`. Khi người dùng đăng nhập, hệ thống lấy lại `iterations`, `salt` và `hash` từ chuỗi đã lưu, băm lại mật khẩu vừa nhập rồi so sánh kết quả. Cách này giúp giảm rủi ro nếu database bị lộ.

**Câu 12. Tại sao OTP không lưu mã gốc mà chỉ lưu hash?**

Đáp án:
OTP cũng là dữ liệu nhạy cảm giống như mật khẩu ngắn hạn. Nếu lưu mã gốc trong database thì bất kỳ ai đọc được DB đều có thể dùng lại mã đó trong thời gian còn hiệu lực. Việc chỉ lưu `CodeHash` giúp giảm rủi ro lộ mã trực tiếp. Khi người dùng nhập OTP, hệ thống băm mã vừa nhập rồi so với hash trong DB.

**Câu 13. Cookie đăng nhập đang chứa gì?**

Đáp án:
Cookie đăng nhập chứa các claim cơ bản như mã người dùng, username, display name, email và role. Nhờ đó, trong suốt thời gian phiên còn hiệu lực, server có thể biết request hiện tại thuộc về ai và người đó có quyền gì. Đây là lý do các controller có thể lấy `CurrentUserId` từ claim thay vì tin dữ liệu do client gửi lên.

**Câu 14. Vì sao phân biệt authentication và authorization là quan trọng?**

Đáp án:
Authentication là trả lời câu hỏi “người dùng là ai”, còn authorization là trả lời câu hỏi “người dùng đó được phép làm gì”. Trong dự án này, cookie login giải quyết phần authentication, còn `[Authorize]` và vai trò `Admin/User` giải quyết phần authorization. Nếu trộn lẫn hai khái niệm này thì rất dễ dẫn đến lỗi bảo mật, ví dụ user đã đăng nhập nhưng lại bị vào nhầm khu admin hoặc admin bị chặn nhầm ở user dashboard.

### 16.4. Ví, giao dịch, gợi ý danh mục

**Câu 15. Vì sao giao dịch là luồng nghiệp vụ quan trọng nhất?**

Đáp án:
Giao dịch là nguồn dữ liệu đầu vào cho gần như toàn bộ phần còn lại của hệ thống. Khi có giao dịch mới, số dư ví thay đổi, ngân sách có thể bị vượt, cảnh báo có thể được sinh ra, dashboard phải cập nhật, báo cáo phải tổng hợp lại và dự báo chi tiêu có thể thay đổi. Vì vậy nếu luồng giao dịch sai thì các module phía sau đều sai theo.

**Câu 16. Vì sao khi sửa giao dịch phải reverse dữ liệu cũ trước rồi mới apply dữ liệu mới?**

Đáp án:
Ví hiện tại đã chứa ảnh hưởng của giao dịch cũ từ trước. Nếu không hoàn tác ảnh hưởng cũ mà chỉ áp dữ liệu mới lên thì số dư sẽ bị cộng dồn sai. Ví dụ một giao dịch chi 100.000 được sửa thành 150.000. Nếu chỉ trừ thêm 150.000 mà không cộng lại 100.000 cũ, ví sẽ bị lệch 100.000.

**Câu 17. `CategorizationService` là AI hay heuristic?**

Đáp án:
Nó là heuristic có học từ hành vi người dùng, chứ chưa phải machine learning đúng nghĩa. Service này dùng hai nguồn dữ liệu chính: bộ từ khóa mặc định theo luật và bộ từ khóa cá nhân học từ lịch sử lựa chọn của chính user. Vì vậy có thể gọi là “thông minh theo luật và dữ liệu hành vi”, nhưng không nên nói quá thành AI phức tạp.

**Câu 18. Vì sao phải normalize tiếng Việt khi gợi ý danh mục?**

Đáp án:
Người dùng có thể nhập note theo nhiều cách khác nhau như có dấu, không dấu, viết hoa, viết thường hoặc gõ sai khoảng trắng. Nếu không chuẩn hóa thì hệ thống sẽ khó nhận ra các từ khóa giống nhau về nghĩa. Bước normalize giúp bỏ dấu, chuyển lowercase và chuẩn hóa chuỗi, từ đó tăng khả năng match rule và keyword cá nhân.

### 16.5. Ngân sách, cảnh báo, realtime

**Câu 19. Luồng cảnh báo vượt ngân sách chạy như thế nào?**

Đáp án:
Sau khi transaction loại `Expense` được lưu thành công, controller gọi `BudgetMonitorService.CheckAndNotifyAsync(...)`. Service tìm budget phù hợp theo `UserId`, `CategoryId`, `Year`, `Month`, sau đó cộng toàn bộ chi tiêu của tháng đó để lấy `spentAmount`, rồi tính `usagePercent`. Nếu vượt điều kiện cảnh báo, service tạo `BudgetAlert`, lưu vào DB, bắn SignalR event và thử đưa email cảnh báo vào luồng gửi.

**Câu 20. Vì sao phải lưu `BudgetAlert` vào database dù đã có SignalR?**

Đáp án:
SignalR chỉ giải quyết phần hiển thị realtime tại thời điểm đó. Nếu user đang redirect, mất kết nối hoặc mở app sau đó thì toast realtime có thể biến mất. Việc lưu `BudgetAlert` vào DB giúp dashboard vẫn hiển thị lịch sử cảnh báo, hỗ trợ đánh dấu đã đọc và tránh mất dữ liệu nghiệp vụ.

**Câu 21. Vì sao `WalletBalanceMonitorService` chỉ cảnh báo khi vừa cắt ngưỡng thấp?**

Đáp án:
Mục tiêu là tránh spam người dùng. Nếu một ví đã dưới 200.000 từ trước rồi, mỗi lần chi thêm 10.000 mà vẫn bắn cảnh báo thì trải nghiệm sẽ rất tệ. Vì vậy service chỉ bắn cảnh báo khi số dư đi từ trạng thái “an toàn” sang “dưới ngưỡng”.

### 16.6. Dashboard, forecast, báo cáo

**Câu 22. Dự báo chi tiêu cuối tháng được tính thế nào?**

Đáp án:
`ForecastService` lấy toàn bộ expense của tháng hiện tại, tính một ngưỡng để tách chi tiêu đột biến, sau đó chia dữ liệu thành `SpikeTotal` và `RoutineTotal`. Phần `RoutineTotal` được nội suy cho toàn tháng theo số ngày quan sát, còn `SpikeTotal` được giữ nguyên theo giá trị thực tế. Cuối cùng, hệ thống kết hợp giá trị này với trung bình 3 tháng gần nhất để giảm dao động.

**Câu 23. Vì sao thuật toán không dùng trung bình cộng đơn giản?**

Đáp án:
Nếu dùng trung bình cộng đơn giản, một khoản chi lớn bất thường như mua laptop, đóng học phí hay sửa xe sẽ làm cho dự báo bị đội lên mạnh và không còn phản ánh thói quen chi tiêu thường ngày nữa. Việc tách chi đột biến giúp hệ thống dự báo phần “chi lặp lại” hợp lý hơn.

**Câu 24. Vì sao tài khoản mới tạo giữa tháng từng bị dự báo sai?**

Đáp án:
Logic cũ lấy `daysPassed` từ đầu tháng, ví dụ ngày 20 thì coi như đã quan sát 20 ngày. Nhưng tài khoản mới tạo ngày 18 thực tế chỉ có 2-3 ngày dữ liệu. Khi đem tổng chi 2 ngày chia cho 20 hoặc nội suy từ tháng đầu mà không tính đến ngày tạo tài khoản, kết quả rất dễ lệch. Bản vá hiện tại dùng `CreatedAt` của user để xác định `observationStart`, tức chỉ nội suy trên khoảng thời gian mà hệ thống thực sự có cơ hội quan sát dữ liệu của tài khoản đó.

**Câu 25. Vì sao `ReportService.BuildSummaryAsync(...)` lại là service trung tâm của phần báo cáo?**

Đáp án:
Service này gom toàn bộ logic tổng hợp dữ liệu: tổng thu, tổng chi, chênh lệch, số lượng giao dịch, trung bình chi mỗi ngày và top category. Khi cùng một logic được dùng cho giao diện báo cáo, export Excel và gửi email, hệ thống tránh được tình trạng mỗi chỗ tính một kiểu. Đây là cách tách business logic đúng tinh thần kiến trúc web nhiều lớp.

### 16.7. Email nền, background service, admin

**Câu 26. Vì sao không gửi email trực tiếp trong request?**

Đáp án:
Gửi email phụ thuộc SMTP, có thể chậm hoặc lỗi mạng. Nếu buộc request web chờ đến khi gửi xong mail thì trải nghiệm người dùng sẽ chậm và dễ timeout. Vì vậy hệ thống chỉ enqueue email trong request, còn worker nền mới là nơi gửi thật.

**Câu 27. `EmailQueue` hoạt động thế nào?**

Đáp án:
`EmailQueue` dùng `Channel<EmailMessage>` làm vùng đệm trong memory. Khi controller hoặc service muốn gửi mail, nó gọi enqueue để đưa `EmailMessage` vào channel. `QueuedEmailBackgroundService` chạy nền liên tục, lấy từng email ra khỏi channel rồi gọi `IEmailSender` để gửi thật.

**Câu 28. Vì sao module admin cần nhiều ràng buộc hơn khu user thường?**

Đáp án:
Admin có thể tác động đến dữ liệu dùng chung của nhiều người như account, category, log hệ thống. Nếu ràng buộc lỏng, chỉ một thao tác sai ở admin có thể làm hỏng dữ liệu diện rộng. Vì vậy các thao tác như xóa category, đổi loại category, khóa user hoặc xem log đều cần thêm điều kiện nghiệp vụ chặt hơn so với khu user thường.

## 17. Câu hỏi bổ sung kiến thức môn Lập trình Web

Phần này là các câu giảng viên có thể hỏi theo đúng kiến thức môn học, nhưng vẫn gắn vào đồ án.

### 17.1. HTTP, request, response

**Câu 29. HTTP là gì và trong dự án này nó xuất hiện ở đâu?**

Đáp án:
HTTP là giao thức trao đổi dữ liệu giữa trình duyệt và web server. Trong dự án này, mỗi lần người dùng truy cập `/Auth/Login`, `/Transactions`, `/Reports` hay gửi form lưu giao dịch thì đều là một HTTP request. Controller nhận request, xử lý và trả về response là HTML, redirect hoặc file Excel.

**Câu 30. Sự khác nhau giữa `GET` và `POST` là gì?**

Đáp án:
`GET` thường dùng để lấy dữ liệu và hiển thị trang, ví dụ mở dashboard hay danh sách giao dịch. `POST` dùng để thay đổi dữ liệu trên server như đăng ký tài khoản, lưu giao dịch, tạo ngân sách hoặc gửi email. Trong đồ án này, phần lớn action `Index(...)` là `GET`, còn `Save(...)`, `Delete(...)`, `Login(...)`, `Register(...)` là `POST`.

**Câu 31. Vì sao xóa giao dịch, xóa ví, lưu ngân sách nên dùng `POST` thay vì `GET`?**

Đáp án:
Vì đây là những thao tác làm thay đổi dữ liệu. Nếu dùng `GET`, người dùng chỉ cần mở link là đã có thể làm thay đổi hệ thống, điều này sai cả về chuẩn web lẫn bảo mật. Dùng `POST` kết hợp anti-forgery token giúp an toàn và đúng ngữ nghĩa hơn.

### 17.2. MVC, model binding, validation

**Câu 32. MVC thể hiện thế nào trong đồ án này?**

Đáp án:
Model là các entity và view model, View là các file Razor trong `Views`, còn Controller là nơi nhận request và điều phối nghiệp vụ. Ví dụ ở chức năng giao dịch, controller nhận form, service hỗ trợ logic gợi ý hoặc budget, rồi cuối cùng render lại view hoặc redirect. Đây là cách áp dụng MVC khá điển hình.

**Câu 33. Model binding là gì và nó đang dùng ở đâu?**

Đáp án:
Model binding là cơ chế tự động ánh xạ dữ liệu từ form hoặc query string thành object C#. Trong đồ án, khi người dùng submit form giao dịch hoặc form budget, ASP.NET Core tự map dữ liệu vào `TransactionFormViewModel` hoặc `BudgetFormViewModel`. Điều này giúp code controller gọn và rõ ràng hơn.

**Câu 34. Validation trong web app này đang diễn ra ở mấy lớp?**

Đáp án:
Ít nhất có ba lớp. Lớp đầu là validation UI hoặc HTML input. Lớp thứ hai là model validation trong controller và view model. Lớp cuối cùng là ràng buộc ở database như unique index, foreign key, delete behavior. Làm nhiều lớp như vậy giúp tránh phụ thuộc hoàn toàn vào frontend.

### 17.3. State management, cookie, session

**Câu 35. Tại sao nói web là stateless và dự án này giải quyết ra sao?**

Đáp án:
Stateless nghĩa là mỗi request HTTP về bản chất độc lập, server không tự nhớ người dùng là ai giữa các request. Dự án giải quyết bằng Cookie Authentication: sau khi đăng nhập, cookie được gửi kèm ở các request sau, từ đó server khôi phục lại identity của user.

**Câu 36. Vì sao dự án dùng cookie auth thay vì JWT?**

Đáp án:
Vì đây là ứng dụng web server-rendered kiểu MVC, cùng domain, cùng server, nên cookie auth đơn giản và phù hợp hơn. JWT thường phù hợp hơn với SPA hoặc mobile app gọi API độc lập. Với đồ án này, cookie auth vừa đúng bài học web app truyền thống vừa giảm độ phức tạp.

### 17.4. Middleware, bảo mật web, CSRF, XSS

**Câu 37. Middleware là gì và trong app này có những middleware nào đáng nhớ?**

Đáp án:
Middleware là các lớp xử lý request theo pipeline. Trong app này có các middleware đáng nhớ như static files, routing, authentication, authorization và status code pages. Chúng chạy theo thứ tự trong `Program.cs` và quyết định request sẽ được xử lý ra sao.

**Câu 38. CSRF là gì và trong đồ án này giảm rủi ro bằng cách nào?**

Đáp án:
CSRF là tấn công lợi dụng trình duyệt của user đã đăng nhập để gửi request thay user. Trong ứng dụng MVC, cách phổ biến để giảm rủi ro là dùng anti-forgery token ở các form `POST`, kết hợp với cookie auth. Nhiều action thay đổi dữ liệu trong dự án có `[ValidateAntiForgeryToken]`.

**Câu 39. XSS là gì và dự án này cần cẩn thận ở đâu?**

Đáp án:
XSS là chèn mã script độc hại vào nội dung hiển thị trên trang. Các chỗ nhận note giao dịch, display name, message log hoặc nội dung nhập từ user đều có rủi ro nếu render sai cách. Razor mặc định encode output, nên an toàn hơn nếu không cố tình render HTML thô.

### 17.5. ORM, async, scalability

**Câu 40. ORM là gì và EF Core mang lại lợi ích gì?**

Đáp án:
ORM là kỹ thuật ánh xạ dữ liệu quan hệ thành object trong code. Với EF Core, nhóm có thể thao tác dữ liệu qua LINQ và entity thay vì viết nhiều SQL thủ công. Điều này giúp tăng tốc độ phát triển và làm code dễ đọc hơn trong môi trường học tập.

**Câu 41. Vì sao gần như toàn bộ controller và service đều dùng `async/await`?**

Đáp án:
Vì phần lớn thao tác như query DB, ghi DB, gửi email hay đọc ghi file đều là I/O. Dùng `async/await` giúp luồng xử lý web không bị block không cần thiết, từ đó ứng dụng phản hồi tốt hơn khi có nhiều request.

**Câu 42. Nếu dữ liệu tăng mạnh, phần nào nên tối ưu trước?**

Đáp án:
Nên tối ưu các chỗ tổng hợp dữ liệu nặng như dashboard, báo cáo, budget spent theo tháng và forecast. Tiếp theo là xem lại index DB và giảm các đoạn load quá nhiều transaction vào memory khi chưa cần thiết. Cuối cùng mới tính đến scale SignalR và thay email queue trong memory bằng queue bền vững hơn.

## 18. Câu hỏi mở rộng để tự luyện thêm

1. Nếu thay SQL Server bằng PostgreSQL thì phần nào của dự án bị ảnh hưởng?
2. Nếu muốn tách dự án thành Web + API + Mobile thì phần auth nên đổi như thế nào?
3. Nếu muốn lưu lịch sử thay đổi budget, nên thêm bảng gì?
4. Nếu muốn dự báo tốt hơn, có thể thay heuristic hiện tại bằng mô hình nào?
5. Nếu muốn chống gửi mail trùng mạnh hơn khi scale nhiều instance, nên dùng gì thay `Channel` trong memory?
6. Nếu muốn realtime ổn định hơn khi deploy nhiều server, nên bổ sung Redis backplane hay một giải pháp tương đương?
7. Nếu muốn tối ưu dashboard, nên đẩy thêm phép group nào xuống SQL thay vì làm trong memory?
8. Nếu user nhập note rất ngắn như “ăn”, “xăng”, “cà phê”, hệ thống gợi ý danh mục hiện tại hoạt động tốt hay còn điểm yếu gì?
