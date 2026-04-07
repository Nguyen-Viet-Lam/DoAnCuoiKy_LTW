# Hướng Dẫn Đọc Code Dự Án DoAnLTW

Tài liệu này giúp bạn hiểu dự án theo góc nhìn code:

- Dự án dùng công nghệ gì, thư viện nào
- Mỗi chức năng nằm ở đâu
- Luồng dữ liệu chạy như thế nào từ request đến database/UI
- Thuật toán và công thức tính toán nằm ở đâu
- Nên đọc file theo thứ tự nào để nắm dự án nhanh

Ghi chú:

- Các line number bên dưới bám theo snapshot code hiện tại của repo.
- Nếu bạn sửa code sau này thì số dòng có thể lệch đi một chút.
- Tài liệu này ưu tiên chỉ ra "đúng chỗ cần đọc" hơn là chép lại toàn bộ code.

## 1. Bức tranh tổng thể

Đây là một ứng dụng quản lý chi tiêu cá nhân viết bằng ASP.NET Core MVC (.NET 8), dùng SQL Server để lưu dữ liệu, Cookie Authentication để đăng nhập, SignalR để đẩy cảnh báo realtime, SMTP để gửi OTP/báo cáo email, và ClosedXML để xuất Excel.

Mục tiêu nghiệp vụ chính:

- Đăng ký, đăng nhập, quên mật khẩu bằng OTP email
- Tạo ví tiền, nhập giao dịch thu/chi
- Đặt ngân sách theo danh mục/tháng
- Theo dõi dashboard tổng quan
- Xem báo cáo, xuất Excel, gửi email
- Có khu admin để quản trị người dùng, danh mục và log hệ thống

## 2. Công nghệ và thư viện đang dùng

### 2.1. Package NuGet trong project

Nguồn đọc: `DoAnLTW.Web/DoAnLTW.Web.csproj`

| Thư viện | Dòng code | Vai trò |
|---|---:|---|
| `.NET 8` | dòng 4 | Nền tảng chạy ứng dụng |
| `ClosedXML` | dòng 13 | Xuất file Excel báo cáo |
| `Microsoft.EntityFrameworkCore.SqlServer` | dòng 14 | Kết nối và query SQL Server |
| `Microsoft.EntityFrameworkCore.Design` | dòng 15-18 | Hỗ trợ migration/tooling EF Core |
| `System.Security.Permissions` | dòng 19 | Thư viện phụ trợ cho API bảo mật/tương thích |
| `UserSecretsId` | dòng 9 | Cho phép lưu secret local khi dev |

### 2.2. Framework/thư viện có sẵn của ASP.NET Core

Nguồn đọc: `DoAnLTW.Web/Program.cs`

- MVC: `AddControllersWithViews()` ở dòng 28
- SignalR: `AddSignalR()` ở dòng 29
- Cookie Authentication: `AddAuthentication(...).AddCookie(...)` ở dòng 49-56
- Data Protection: dòng 33-41
- DI container: đăng ký service ở dòng 60-73
- SQL Server + EF Core: `UseSqlServer(...)` ở dòng 46-47

### 2.3. Frontend library

Nguồn đọc: `DoAnLTW.Web/Views/Shared/_Layout.cshtml`

| Thư viện | Dòng code | Vai trò |
|---|---:|---|
| Bootstrap CSS | dòng 21 | UI layout/component |
| Bootstrap Icons CDN | dòng 22 | Icon giao diện |
| CSS riêng `site.css` | dòng 23 | Toàn bộ style custom |
| jQuery | dòng 231 | Hỗ trợ script view/validation |
| Bootstrap JS | dòng 232 | Dropdown, toast, modal, responsive UI |
| SignalR browser client | dòng 233 | Nhận cảnh báo realtime |
| JS riêng `site.js` | dòng 234 | Sidebar, xác nhận submit, realtime toast |

## 3. Cấu trúc thư mục để đọc nhanh

### 3.1. Phần quan trọng nhất

- `DoAnLTW.Web/Program.cs`
  - Điểm khởi động app, đăng ký DI, middleware, route, SignalR hub
- `DoAnLTW.Web/Data`
  - `FinanceDbContext`, seed dữ liệu, migrations
- `DoAnLTW.Web/Models/Entities`
  - Mô hình bảng dữ liệu
- `DoAnLTW.Web/Controllers`
  - Nơi điều phối luồng nghiệp vụ theo từng màn hình/chức năng
- `DoAnLTW.Web/Services`
  - Chứa nghiệp vụ tách riêng: OTP, báo cáo, gợi ý danh mục, dự báo, email, log...
- `DoAnLTW.Web/Views`
  - Razor view, có một phần logic hiển thị và tính toán hình vẽ đơn giản
- `DoAnLTW.Web/wwwroot`
  - JS/CSS tĩnh, upload avatar

### 3.2. Nếu chỉ có 15 phút để đọc dự án

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
12. `DoAnLTW.Web/Services/Email/*`

## 4. App khởi động như thế nào

Nguồn đọc chính: `DoAnLTW.Web/Program.cs`

### 4.1. Các bước khởi động

1. Đọc cấu hình từ `appsettings` và environment:
   - Connection string ở dòng 14
   - `SeedDemoData` ở dòng 15
   - `SeedShowcaseUserEmail` ở dòng 16

2. Nếu thiếu `DefaultConnection` thì app ném lỗi ngay:
   - dòng 18-21

3. Bật MVC, SignalR, HttpContextAccessor:
   - dòng 28-30

4. Tạo thư mục lưu Data Protection key:
   - `App_Data/DataProtectionKeys`, dòng 33-41

5. Bind options:
   - `SmtpOptions` ở dòng 43
   - `AdminSeedOptions` ở dòng 44

6. Đăng ký `FinanceDbContext` với SQL Server:
   - dòng 46-47

7. Đăng ký Cookie Authentication:
   - dòng 49-56
   - `LoginPath = /Auth/Login` ở dòng 52
   - `AccessDeniedPath = /Auth/AccessDenied` ở dòng 53
   - `ExpireTimeSpan = 14 ngày` ở dòng 55

8. Đăng ký tất cả service nghiệp vụ vào DI:
   - dòng 60-73

9. Khi app build xong, gọi seed dữ liệu:
   - `SeedData.InitializeAsync(...)` ở dòng 77

10. Cấu hình middleware:
   - status code pages ở dòng 86
   - static files ở dòng 87
   - routing ở dòng 88
   - authentication ở dòng 89
   - authorization ở dòng 90

11. Map SignalR hub:
   - `/budgetHub` ở dòng 92

12. Map route mặc định MVC:
   - dòng 94-96

### 4.2. Ý nghĩa kiến trúc

`Program.cs` là nơi nối tất cả khối của hệ thống:

- Database
- Auth
- Service nghiệp vụ
- Background worker
- Realtime hub
- Routing web

Nói ngắn gọn: đây là file "dây điện trung tâm" của dự án.

## 5. Cấu hình hệ thống đang nằm ở đâu

### 5.1. appsettings

Nguồn đọc:

- `DoAnLTW.Web/appsettings.json`
- `DoAnLTW.Web/appsettings.Development.json`

Các section chính:

- `ConnectionStrings` ở dòng 2
- `AdminAccount` ở dòng 5
- `Smtp` ở dòng 11
- `SeedDemoData` ở dòng 26
- `AllowedHosts` ở `appsettings.json` dòng 27

### 5.2. Option class bind vào config

Nguồn đọc:

- `DoAnLTW.Web/Models/Options/SmtpOptions.cs`
- `DoAnLTW.Web/Models/Options/AdminSeedOptions.cs`

Ý nghĩa:

- `SmtpOptions`: host, port, SSL, username, password, from email/name
- `AdminSeedOptions`: tài khoản admin mặc định khi seed

## 6. Database model và quan hệ

### 6.1. DbContext

Nguồn đọc: `DoAnLTW.Web/Data/FinanceDbContext.cs`

DbSet chính:

- `Roles` dòng 12
- `Users` dòng 13
- `EmailOtps` dòng 14
- `Wallets` dòng 15
- `Categories` dòng 16
- `Transactions` dòng 17
- `Budgets` dòng 18
- `UserPersonalKeywords` dòng 19
- `BudgetAlerts` dòng 20
- `SystemLogs` dòng 21
- `ReportDispatchLogs` dòng 22

`OnModelCreating(...)` bắt đầu ở dòng 24.

### 6.2. Entity chính và vai trò

Nguồn đọc:

- `DoAnLTW.Web/Models/Entities/IdentityEntities.cs`
- `DoAnLTW.Web/Models/Entities/FinanceEntities.cs`
- `DoAnLTW.Web/Models/Entities/SystemEntities.cs`

| Entity | Dòng bắt đầu | Ý nghĩa |
|---|---:|---|
| `Role` | `IdentityEntities.cs:6` | Vai trò `Admin` / `User` |
| `AppUser` | `IdentityEntities.cs:17` | Người dùng hệ thống |
| `EmailOtp` | `IdentityEntities.cs:62` | OTP email cho đăng ký/đổi mật khẩu |
| `Wallet` | `FinanceEntities.cs:6` | Ví tiền |
| `Category` | `FinanceEntities.cs:38` | Danh mục thu/chi |
| `WalletTransaction` | `FinanceEntities.cs:67` | Giao dịch |
| `Budget` | `FinanceEntities.cs:108` | Ngân sách theo tháng/danh mục |
| `UserPersonalKeyword` | `FinanceEntities.cs:136` | Từ khóa cá nhân hệ thống học được |
| `BudgetAlert` | `FinanceEntities.cs:158` | Cảnh báo ngân sách |
| `SystemLog` | `SystemEntities.cs:6` | Nhật ký hệ thống |
| `ReportDispatchLog` | `SystemEntities.cs:31` | Log gửi báo cáo |

### 6.3. Các index quan trọng

Nguồn đọc: `DoAnLTW.Web/Data/FinanceDbContext.cs`

- Role unique theo `Name`: dòng 29-30
- User unique theo `Username`: dòng 33-34
- User unique theo `Email`: dòng 37-38
- Budget unique theo `(UserId, CategoryId, Year, Month)`: dòng 44-45
- Personal keyword unique theo `(UserId, Keyword)`: dòng 48-49
- Report dispatch unique theo `(UserId, ReportType, PeriodKey)`: dòng 52-53

Ý nghĩa:

- Tránh trùng username/email
- Mỗi user chỉ có 1 budget cho 1 danh mục trong 1 tháng
- Không gửi báo cáo tuần trùng kỳ cho cùng user

### 6.4. Quan hệ và hành vi xóa

Nguồn đọc: `DoAnLTW.Web/Data/FinanceDbContext.cs`

Ví dụ đáng chú ý:

- `AppUser -> Role`: `Restrict`, dòng 58-62
- `Wallet -> User`: `Cascade`, dòng 70-74
- `Transaction -> Wallet`: `Restrict`, dòng 88-92
- `Transaction -> Category`: `Restrict`, dòng 94-98
- `Budget -> User`: `Cascade`, dòng 100-104
- `BudgetAlert -> User/Budget`: `NoAction`, dòng 124-134
- `SystemLog -> User`: `SetNull`, dòng 142-146

Ý nghĩa dễ nhớ:

- Dữ liệu cốt lõi như giao dịch không cho xóa dây chuyền bừa bãi nếu còn quan hệ quan trọng
- Một số log vẫn giữ lại dù user bị xóa

## 7. Seed dữ liệu chạy như thế nào

### 7.1. Entry point seed

Nguồn đọc: `DoAnLTW.Web/Data/SeedData.cs`

- `InitializeAsync(...)` ở dòng 8-27
- Chạy `db.Database.MigrateAsync()` ở dòng 14
- Sau đó seed reference data ở dòng 17
- Có thể seed demo data ở dòng 19-22

### 7.2. Seed role, category mặc định, admin

Nguồn đọc: `DoAnLTW.Web/Data/Seeders/ReferenceDataSeeder.cs`

- `SeedAsync(...)` ở dòng 11-69
- Danh mục mặc định ở `GetDefaultCategories()` dòng 71-90
- Đồng bộ danh mục mặc định ở `SyncDefaultCategoriesAsync(...)` dòng 92-121

Bạn cần nhớ:

- Role `Admin`, `User` được tạo ở đây
- Admin mặc định cũng được tạo hoặc cập nhật ở đây
- Category mặc định của hệ thống cũng nằm ở đây

### 7.3. Seed demo user, ví, giao dịch, budget, alert, log

Nguồn đọc: `DoAnLTW.Web/Data/Seeders/DemoFinanceDataSeeder.cs`

Các method đáng chú ý:

- `SeedAsync(...)`: dòng 11-175
- `SeedShowcaseUserAsync(...)`: dòng 178-196
- `EnsureUserAsync(...)`: dòng 199-232
- `EnsurePrimaryDemoProfileAsync(...)`: dòng 293-566
- `EnsureMaleDemoProfileAsync(...)`: dòng 568-711
- `EnsureShowcaseYearProfileAsync(...)`: dòng 713-907
- `EnsureWalletAsync(...)`: dòng 1162-1200
- `EnsureTransactionAsync(...)`: dòng 1202-1245
- `EnsureBudgetAsync(...)`: dòng 1247-1283
- `RecalculateWalletBalancesAsync(...)`: dòng 1285-1299
- `SeedAlertsAsync(...)`: dòng 1301-1327
- `SeedLogsAsync(...)`: dòng 1329-1363

Điểm rất quan trọng:

- Sau khi seed giao dịch, hàm `RecalculateWalletBalancesAsync(...)` tính lại số dư ví theo công thức:
  - `CurrentBalance = InitialBalance + Income - Expense`
  - xem `DemoFinanceDataSeeder.cs:1290-1297`

## 8. Xác thực và bảo mật

### 8.1. Mật khẩu được lưu thế nào

Nguồn đọc: `DoAnLTW.Web/Services/Security/PasswordService.cs`

- Hằng số bảo mật:
  - `SaltSize = 16` ở dòng 7
  - `HashSize = 32` ở dòng 8
  - `Iterations = 100_000` ở dòng 9
- Hash password:
  - `Hash(...)` ở dòng 11-16
  - dùng `Rfc2898DeriveBytes.Pbkdf2(..., SHA256, ...)` ở dòng 14
- Verify password:
  - `Verify(...)` ở dòng 18-36
  - so sánh an toàn bằng `FixedTimeEquals(...)` ở dòng 36

Kết luận:

- Không lưu plain text password
- Có salt riêng
- Dùng PBKDF2 + SHA256
- Có chống timing attack khi so sánh

### 8.2. OTP hoạt động thế nào

Nguồn đọc: `DoAnLTW.Web/Services/Auth/OtpService.cs`

- Tạo mã OTP 6 số ngẫu nhiên:
  - `RandomNumberGenerator.GetInt32(100000, 999999)` ở dòng 23
- Không lưu mã gốc, chỉ lưu hash:
  - `CodeHash = ComputeHash(code)` ở dòng 30
- OTP hết hạn sau 10 phút:
  - `ExpiresAt = DateTime.UtcNow.AddMinutes(10)` ở dòng 31
- Mỗi lần verify tăng số lần thử:
  - `otp.Attempts += 1` ở dòng 77
- Quá 5 lần sai thì chặn:
  - dòng 79-83
- So sánh hash OTP:
  - dòng 85-90
- Hàm hash OTP:
  - `ComputeHash(...)` ở dòng 96-100, dùng SHA256

### 8.3. Cookie login chứa gì

Nguồn đọc: `DoAnLTW.Web/Services/Auth/CookieAuthService.cs`

- Tạo claims ở dòng 12-18:
  - `NameIdentifier`
  - `Name`
  - `GivenName`
  - `Email`
  - `Role`
- Ký cookie ở dòng 24-31
- Hạn cookie:
  - nhớ đăng nhập thì 14 ngày
  - không nhớ đăng nhập thì 1 ngày
  - xem dòng 30

### 8.4. Lấy `CurrentUserId` ở đâu

Nguồn đọc:

- `DoAnLTW.Web/Extensions/ClaimsPrincipalExtensions.cs:7-11`
- `DoAnLTW.Web/Controllers/AppControllerBase.cs:10`

Ý tưởng:

- Đọc `ClaimTypes.NameIdentifier`
- Parse thành `int`
- Mọi controller kế thừa `AppControllerBase` sẽ dùng `CurrentUserId`

## 9. Luồng chức năng theo từng module

### 9.1. Auth: đăng ký, đăng nhập, OTP, quên mật khẩu

Nguồn đọc: `DoAnLTW.Web/Controllers/AuthController.cs`

### Đăng nhập

- GET login: dòng 36-47
- POST login: dòng 49-83
- Kiểm tra password:
  - `_passwordService.Verify(...)` ở dòng 61
- Kiểm tra tài khoản bị khóa:
  - dòng 67-71
- Kiểm tra email đã xác thực chưa:
  - dòng 73-78
- Tạo cookie đăng nhập:
  - dòng 80

### Đăng ký

- GET register: dòng 85-90
- POST register: dòng 93-139
- Check trùng email:
  - dòng 102-106
- Check trùng username:
  - dòng 108-112
- Hash password và tạo user:
  - dòng 116-130
- Gửi OTP xác thực:
  - dòng 133
- Ghi audit log:
  - dòng 134

### Xác thực OTP

- GET verify page: dòng 141-151
- POST verify OTP: dòng 153-182
- Gọi service verify:
  - dòng 160
- Nếu purpose là `Register` thì bật `IsEmailVerified = true`:
  - dòng 167-175

### Gửi lại OTP

- `ResendOtp(...)`: dòng 184-192

### Quên mật khẩu / đặt lại mật khẩu

- GET forgot password: dòng 194-199
- POST forgot password: dòng 202-219
- GET reset password: dòng 221-226
- POST reset password: dòng 229-258
- Verify OTP trước:
  - dòng 236
- Hash password mới:
  - dòng 251

### Redirect theo role sau đăng nhập

- `RedirectToLocal(...)`: dòng 273-284
- Admin vào `Admin/Dashboard`
- User vào `Dashboard/Index`

### 9.2. Wallet: quản lý ví

Nguồn đọc: `DoAnLTW.Web/Controllers/WalletsController.cs`

### Chức năng

- Danh sách ví: `Index(...)` dòng 28-69
- Tạo/sửa ví: `Save(...)` dòng 73-122
- Xóa ví: `Delete(...)` dòng 124-146

### Logic đáng chú ý

- Khi sửa `InitialBalance`, controller tính delta:
  - `initialBalanceDelta = model.InitialBalance - existing.InitialBalance` ở dòng 87
- Sau đó cộng delta vào `CurrentBalance`:
  - dòng 92

Ý nghĩa:

- Nếu user chỉnh vốn ban đầu thì số dư hiện tại cũng được hiệu chỉnh tương ứng

- Không cho xóa ví nếu ví đã có giao dịch:
  - `wallet.Transactions.Any()` ở dòng 133

- Đánh dấu ví sắp hết tiền:
  - `IsLowBalance(...)` gọi ở dòng 41 và 160

### 9.3. Transaction: luồng nghiệp vụ quan trọng nhất

Nguồn đọc chính:

- `DoAnLTW.Web/Controllers/TransactionsController.cs`
- `DoAnLTW.Web/Views/Transactions/Index.cshtml`
- `DoAnLTW.Web/Services/Finance/CategorizationService.cs`

### Các action chính

- Trang giao dịch + filter: `Index(...)` dòng 34-57
- Tạo/sửa giao dịch: `Save(...)` dòng 59-180
- Xóa giao dịch: `Delete(...)` dòng 183-210
- API gợi ý danh mục: `SuggestCategory(...)` dòng 212-230
- Build dữ liệu trang: `BuildPageModelAsync(...)` dòng 232-372

### Luồng khi lưu giao dịch mới

1. Validate model
2. Check category hợp lệ theo `Type`
   - dòng 66-74
3. Gọi AI nhẹ để gợi ý category
   - `_categorizationService.SuggestAsync(...)` ở dòng 76
4. Load ví của user
   - dòng 142-148
5. Tạo `WalletTransaction`
   - dòng 152-166
6. Cập nhật số dư ví
   - `ApplyWalletBalance(...)` ở dòng 168
7. Save DB
   - dòng 170
8. Học lại từ note nếu user chọn category khác gợi ý
   - dòng 172
9. Kiểm tra ngân sách
   - dòng 173
10. Kiểm tra ví có rơi xuống ngưỡng thấp không
    - dòng 174
11. Ghi log
    - dòng 175

### Luồng khi sửa giao dịch

Phần này khá quan trọng vì có hoàn tác số dư cũ:

- Tìm giao dịch hiện có + wallet gốc: dòng 78-83
- Nếu đổi ví, tìm ví đích: dòng 85-95
- Lưu số dư cũ để so ngưỡng cảnh báo: dòng 98-101
- Hoàn tác ảnh hưởng giao dịch cũ:
  - `ReverseWalletBalance(...)` ở dòng 107
- Cập nhật transaction:
  - dòng 109-119
- Áp dụng ảnh hưởng giao dịch mới:
  - `ApplyWalletBalance(...)` ở dòng 119
- Xóa alert ngân sách cũ gắn với transaction này:
  - dòng 121-124
- Save và check lại budget/wallet:
  - dòng 126-133

### Luồng khi xóa giao dịch

- Load transaction + wallet: dòng 185-188
- Hoàn tác số dư ví:
  - dòng 195
- Xóa budget alert liên quan:
  - dòng 197-201
- Xóa transaction:
  - dòng 202
- Save DB:
  - dòng 203

### Bộ lọc và timeline

Trong `BuildPageModelAsync(...)`:

- Filter theo ví: dòng 325-328
- Filter theo category: dòng 330-333
- Filter theo keyword ghi chú/danh mục: dòng 333-339
- Filter theo ngày bắt đầu: dòng 341-344
- Filter theo ngày kết thúc: dòng 346-349
- Lấy tối đa 80 giao dịch: dòng 353-356
- Tính tổng thu tháng hiện tại: dòng 365-367
- Tính tổng chi tháng hiện tại: dòng 368-370

### Công thức cập nhật số dư ví

Nguồn đọc: `TransactionsController.cs`

- `ApplyWalletBalance(...)` dòng 374-384
- `ReverseWalletBalance(...)` dòng 386-396

Công thức:

- Nếu là `Expense`:
  - thêm giao dịch: `CurrentBalance -= amount`
  - hoàn tác: `CurrentBalance += amount`
- Nếu là `Income`:
  - thêm giao dịch: `CurrentBalance += amount`
  - hoàn tác: `CurrentBalance -= amount`

### API gợi ý danh mục từ note

- Action JSON: `SuggestCategory(...)` dòng 212-230
- JS gọi API khi blur ô note:
  - `Views/Transactions/Index.cshtml:256-301`
- `fetch(...)` ở dòng 272
- Tự set category select theo gợi ý ở dòng 286-288

Đây là phần "AI nhẹ" của dự án, nhưng bản chất không dùng mô hình AI nặng mà là rule-based + học từ lịch sử user.

### 9.4. CategorizationService: gợi ý danh mục thông minh

Nguồn đọc: `DoAnLTW.Web/Services/Finance/CategorizationService.cs`

### Dữ liệu rule

- `ExpenseRules`: dòng 12-35
- `IncomeRules`: dòng 37-46

Ví dụ:

- `xang`, `grab`, `taxi` -> `Di chuyển`
- `cafe`, `com`, `bun` -> `Ăn uống`
- `luong`, `salary` -> `Lương`

### Thuật toán gợi ý

Method chính: `SuggestAsync(...)` dòng 55-107

Luồng xử lý:

1. Nếu note rỗng thì bỏ qua:
   - dòng 57-60
2. Normalize text:
   - dòng 62
3. Tách keyword:
   - dòng 63
4. Load keyword cá nhân đã học của user:
   - dòng 65-70
5. Ưu tiên match keyword cá nhân:
   - dòng 72-79
6. Nếu chưa match thì load category hệ thống:
   - dòng 81-87
7. Chọn bộ rule theo `Income` hay `Expense`:
   - dòng 89-91
8. Match từ khóa rule:
   - dòng 93-104

### Thuật toán học lại từ hành vi user

Method: `LearnAsync(...)` dòng 109-157

Ý tưởng:

- Nếu hệ thống gợi ý đúng thì không cần học thêm:
  - dòng 118-123
- Nếu user chọn khác gợi ý, hệ thống lấy tối đa 5 từ khóa:
  - dòng 126-130
- Nếu keyword chưa có thì insert mới:
  - dòng 136-145
- Nếu keyword đã có thì update category + tăng `HitCount`:
  - dòng 147-152

### Normalize text hoạt động thế nào

- `Normalize(...)`: dòng 159-168
- `ExtractKeywords(...)`: dòng 170-174
- `RemoveDiacritics(...)`: dòng 176-198

Điểm hay:

- Bỏ dấu tiếng Việt để match dễ hơn
- Chuyển về lowercase
- Chuẩn hóa khoảng trắng
- Tách từ theo regex

### 9.5. Budget: đặt hạn mức và theo dõi mức sử dụng

Nguồn đọc: `DoAnLTW.Web/Controllers/BudgetsController.cs`

### Chức năng

- Trang ngân sách: `Index(...)` dòng 23-48
- Lưu ngân sách: `Save(...)` dòng 50-106
- Xóa ngân sách: `Delete(...)` dòng 108-124
- Build model hiển thị tiến độ: `BuildPageModelAsync(...)` dòng 126-181

### Kiểm tra ngân sách trùng

- Khi lưu, controller check `(UserId, CategoryId, Year, Month)` khác `Id` hiện tại:
  - dòng 58-64

### Công thức % sử dụng ngân sách

- Trong `BudgetsController`, `UsagePercent = spent / limit * 100` ở dòng 170
- Tổng ngân sách toàn trang:
  - `TotalBudget = items.Sum(x => x.LimitAmount)` ở dòng 180
- Tổng chi trên các budget:
  - `TotalSpent = items.Sum(x => x.SpentAmount)` ở dòng 181

### 9.6. BudgetMonitorService: tạo cảnh báo ngân sách realtime + email

Nguồn đọc: `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs`

### Method chính

- `CheckAndNotifyAsync(...)`: dòng 23-100
- `TrySendBudgetAlertEmailAsync(...)`: dòng 103-172
- `BuildBudgetAlertEmailHtml(...)`: dòng 174 trở đi

### Luồng xử lý

1. Load transaction vừa lưu:
   - dòng 25-30
2. Chỉ xử lý nếu là `Expense`:
   - dòng 32-35
3. Tìm budget đúng user + category + tháng:
   - dòng 40-45
4. Tính `spentAmount` tháng đó:
   - dòng 47-54
5. Tính `usagePercent`:
   - dòng 56
6. Nếu chưa chạm ngưỡng cảnh báo thì dừng:
   - dòng 57-60
7. Nếu alert cho transaction này đã tồn tại thì dừng:
   - dòng 62-67
8. Tạo `BudgetAlert`:
   - dòng 72-83
9. Gửi SignalR event `budgetAlert`:
   - dòng 90-98
10. Thử gửi email cảnh báo:
    - dòng 100

### Công thức chính

- `usagePercent = spentAmount / budget.LimitAmount * 100`
  - `BudgetMonitorService.cs:56`
- `isExceeded = usagePercent >= 100`
  - dòng 69

### Điểm hay của phần email cảnh báo

Trong `TrySendBudgetAlertEmailAsync(...)`:

- Hệ thống kiểm tra đã gửi email cho "mức cảnh báo" này chưa:
  - cùng budget, đã `IsEmailSent`, cùng level cảnh báo
  - logic nằm trong dòng 114-133

Điều đó giúp:

- Không spam email cho cùng một budget mỗi lần user thêm giao dịch
- Chỉ gửi lần đầu cho mức "sắp vượt" hoặc "đã vượt"

### 9.7. WalletBalanceMonitorService: cảnh báo ví sắp hết tiền

Nguồn đọc: `DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs`

- Ngưỡng thấp mặc định:
  - `LowBalanceThreshold = 200000m` ở dòng 9
- Check ví thấp:
  - `IsLowBalance(...)` dòng 18-21
- Realtime alert:
  - `NotifyIfThresholdCrossedAsync(...)` dòng 23-44

Điểm quan trọng:

- Service chỉ bắn cảnh báo khi ví vừa "cắt qua ngưỡng" từ trên xuống dưới
- Điều này được thể hiện ở điều kiện chặn ở dòng 29-32

Nói đơn giản:

- Nếu ví đã thấp từ trước rồi thì không bắn lại liên tục

### 9.8. Dashboard: tổng quan và biểu đồ

Nguồn đọc chính:

- `DoAnLTW.Web/Controllers/DashboardController.cs`
- `DoAnLTW.Web/Views/Dashboard/Index.cshtml`
- `DoAnLTW.Web/Models/ViewModels/Dashboard/MonthlyComparisonItem.cs`

### Chức năng controller

- `Index(...)` dòng 27-183
- `MarkAlertAsRead(...)` dòng 185-199
- `MarkAllAlertsAsRead(...)` dòng 201-218
- `BuildBudgetProgressAsync(...)` dòng 220-251

### Các phép tổng hợp chính trong controller

- Lấy 12 tháng gần nhất:
  - `Enumerable.Range(0, 12)` ở dòng 51
- Group chi theo danh mục:
  - `.GroupBy(...)` ở dòng 79
- Tạo 7 ngày chi gần nhất:
  - `Enumerable.Range(0, 7)` ở dòng 99
- Tổng số dư tất cả ví:
  - `TotalBalance = wallets.Sum(x => x.CurrentBalance)` ở dòng 122
- Dự báo cuối tháng:
  - `ForecastExpense = forecast` ở dòng 130
- Tỷ lệ tiết kiệm:
  - dòng 131
- Chiều cao cột bar chart:
  - `HeightPercent = x.Amount / maxBar * 100` ở dòng 144
- Top budget highlight:
  - sort theo `UsagePercent`, lấy 4 ở dòng 251

### Công thức tỷ lệ tiết kiệm

Nguồn đọc:

- `DashboardController.cs:131`
- `DashboardViewModel.cs:15`

Công thức:

`SavingsRate = (TotalIncomeMonth - TotalExpenseMonth) / TotalIncomeMonth * 100`

Nếu thu nhập tháng bằng 0 thì trả về 0 để tránh chia cho 0.

### Logic hiển thị ở view cũng khá quan trọng

Nguồn đọc: `DoAnLTW.Web/Views/Dashboard/Index.cshtml`

- Tạo polyline cho đường chi tiêu:
  - `expensePolyline` ở dòng 28
- Tạo pie chart bằng CSS `conic-gradient(...)`:
  - dòng 63
- Render đường polyline:
  - dòng 118
- Dùng `selectedMonth.NetAmount`:
  - dòng 188, 192
- Hiển thị `SavingsRate`:
  - dòng 209

### `MonthlyComparisonItem` có gì

Nguồn đọc: `DoAnLTW.Web/Models/ViewModels/Dashboard/MonthlyComparisonItem.cs`

- `ShortLabel` ở dòng 11
- `FullLabel` ở dòng 12
- `NetAmount = Income - Expense` ở dòng 13

### 9.9. ForecastService: dự báo chi tiêu cuối tháng

Nguồn đọc: `DoAnLTW.Web/Services/Finance/ForecastService.cs`

### Method chính

- `CalculateMonthEndExpenseForecastAsync(...)`: dòng 15-66

### Ý tưởng thuật toán

Thuật toán này không chỉ lấy trung bình đơn giản, mà tách khoản chi bất thường ra riêng:

1. Lấy tất cả chi tiêu tháng hiện tại:
   - dòng 22-30
2. Nếu tháng hiện tại chưa có chi tiêu:
   - lấy trung bình 3 tháng trước
   - dòng 32-34
3. Nếu đã có dữ liệu:
   - tính `spikeThreshold`
   - dòng 38
4. Tách giao dịch thành:
   - `spikeTotal`: các khoản chi đột biến
   - `routineTotal`: các khoản chi thường xuyên
   - dòng 40-52
5. Dự báo phần chi thường xuyên:
   - `routineForecast = routineTotal / daysPassed * daysInMonth`
   - dòng 54
6. Dự báo tháng hiện tại:
   - `currentForecast = spikeTotal + routineForecast`
   - dòng 55
7. Lấy trung bình tháng quá khứ:
   - dòng 58
8. Trộn 2 nguồn:
   - `0.65 * currentForecast + 0.35 * averagePastMonths`
   - dòng 65

### Tính ngưỡng chi đột biến

- `CalculateSpikeThreshold(...)`: dòng 68-91
- `GetPercentile(...)`: dòng 93-109

Thuật toán:

- Tính `median`
- Tính `Q1`, `Q3`
- Tính `IQR = Q3 - Q1`
- Tính `iqrThreshold = Q3 + 1.5 * IQR`
- Tính `minThreshold = median * 3`
- Chọn `threshold = max(iqrThreshold, minThreshold, 500000)`

Ý nghĩa:

- Các khoản chi quá lớn sẽ không bị "trung bình hóa" như chi thường ngày
- Dự báo cuối tháng thực tế hơn khi có một vài giao dịch đột biến

### Trung bình 3 tháng trước

- `GetAverageMonthlyExpenseAsync(...)`: dòng 111-120

### 9.10. Reports: tổng hợp, Excel, email

Nguồn đọc:

- `DoAnLTW.Web/Controllers/ReportsController.cs`
- `DoAnLTW.Web/Services/Finance/ReportService.cs`
- `DoAnLTW.Web/Models/ViewModels/Reports/ReportSummaryViewModel.cs`

### Chức năng controller

- Trang báo cáo: `Index(...)` dòng 26-60
- Xuất Excel: `ExportExcel(...)` dòng 62-97
- Gửi báo cáo tuần: `SendWeeklyToEmail(...)` dòng 99-124
- Gửi báo cáo theo khoảng ngày: `SendSummaryToEmail(...)` dòng 126-151

### Tổng hợp báo cáo

Method: `ReportService.BuildSummaryAsync(...)` dòng 17-65

Tính:

- Khoảng ngày inclusive bằng cách dùng `endExclusive = to + 1 ngày`
  - dòng 19
- `TotalIncome`
  - dòng 27
- `TotalExpense`
  - dòng 28
- `BalanceChange = totalIncome - totalExpense`
  - dòng 38
- `TransactionCount`
  - dòng 39
- `AverageExpensePerDay = totalExpense / totalDays`
  - dòng 40
- `TopCategories`
  - dòng 41-49

### Xuất Excel

Method: `ExportExcelAsync(...)` dòng 67-111

Điểm cần đọc:

- Tạo workbook: dòng 71-73
- Ghi summary: dòng 75-82
- Ghi header table giao dịch: dòng 84-89
- Ghi từng transaction: dòng 92-100
- Auto fit column: dòng 104
- Format ngày: dòng 105
- Format số tiền: dòng 106

### Gửi báo cáo email

Method: `BuildWeeklyEmailAsync(...)` dòng 113-146

- Nối HTML top category ở dòng 116-118
- Dùng lại `BuildSummaryAsync(...)` ở dòng 115

### `ReportSummaryViewModel`

Nguồn đọc: `DoAnLTW.Web/Models/ViewModels/Reports/ReportSummaryViewModel.cs`

- `AverageExpensePerDay` nằm ở dòng 11
- `TopCategories` dòng 12
- `Transactions` dòng 13

### 9.11. Profile: hồ sơ cá nhân và avatar

Nguồn đọc:

- `DoAnLTW.Web/Controllers/ProfileController.cs`
- `DoAnLTW.Web/Services/Finance/AvatarStorageService.cs`

### Luồng update profile

- GET profile: `Index(...)` dòng 31-36
- POST profile: `Index(ProfileViewModel ...)` dòng 39-88

Trong POST:

- Check trùng email:
  - dòng 48-55
- Update `DisplayName`, `Email`
  - dòng 57-58
- Lưu avatar:
  - dòng 59
- Nếu người dùng muốn đổi mật khẩu:
  - verify mật khẩu cũ ở dòng 63-70
  - hash mật khẩu mới ở dòng 81
- Save DB:
  - dòng 84

### Avatar được lưu thế nào

Nguồn đọc: `DoAnLTW.Web/Services/Finance/AvatarStorageService.cs`

- Method `SaveAsync(...)`: dòng 12-43
- Chỉ cho phép đuôi:
  - `.jpg`, `.jpeg`, `.png`, `.webp`
  - dòng 19-20
- Tạo thư mục:
  - `wwwroot/uploads/avatars`, dòng 26
- Nếu có avatar cũ thì xóa file cũ:
  - dòng 29-35
- Lưu file mới:
  - dòng 37-41

### 9.12. Admin: dashboard quản trị, user, category, logs

Nguồn đọc: `DoAnLTW.Web/Controllers/AdminController.cs`

Các action chính:

- `Dashboard(...)`: dòng 28-81
- `Users(...)`: dòng 83-146
- `Categories(...)`: dòng 148-200
- `Logs(...)`: dòng 202-238
- `ToggleUser(...)`: dòng 240-267
- `SaveCategory(...)`: dòng 269-347
- `DeleteCategory(...)`: dòng 349-378
- `BuildLogQuery(...)`: dòng 380-391

Điểm cần nhớ:

- Admin không thể khóa admin khác từ màn hình này:
  - dòng 248-253
- Không cho đổi `TransactionType` của category nếu category đã có usage:
  - dòng 300-314
- Không cho xóa category nếu đã có transaction hoặc budget:
  - dòng 359-365

## 10. Email queue và background service

Nguồn đọc:

- `DoAnLTW.Web/Services/Email/EmailMessage.cs`
- `DoAnLTW.Web/Services/Email/EmailQueue.cs`
- `DoAnLTW.Web/Services/Email/QueuedEmailBackgroundService.cs`
- `DoAnLTW.Web/Services/Email/SmtpEmailSender.cs`

### 10.1. Model email

- `EmailMessage` là DTO đơn giản:
  - `To`, `Subject`, `HtmlBody`
  - xem `EmailMessage.cs:3-7`

### 10.2. Hàng đợi email

- `EmailQueue` dùng `Channel<EmailMessage>`:
  - dòng 7
- `QueueAsync(...)`:
  - dòng 9-12
- `DequeueAsync(...)`:
  - dòng 14-17

Ý nghĩa:

- Request web chỉ "đẩy mail vào queue", không chờ SMTP gửi xong
- App phản hồi nhanh hơn

### 10.3. Worker gửi mail nền

- `QueuedEmailBackgroundService.ExecuteAsync(...)`: dòng 19-38
- Worker chờ `DequeueAsync(...)` ở dòng 25
- Tạo scope DI mới rồi resolve `IEmailSender` ở dòng 26-27
- Gọi `SendAsync(...)` ở dòng 28

### 10.4. Gửi SMTP thật hoặc fallback local

Nguồn đọc: `DoAnLTW.Web/Services/Email/SmtpEmailSender.cs`

- `SendAsync(...)`: dòng 25-69
- Nếu thiếu cấu hình SMTP:
  - lưu bản HTML local trong development
  - dòng 27-40
- Gửi SMTP thật:
  - tạo `SmtpClient` ở dòng 43-47
  - tạo `MailMessage` ở dòng 49-58
  - `SendMailAsync(...)` ở dòng 62
- Nếu dev mà SMTP fail:
  - fallback lưu file HTML ở dòng 64-68

### 10.5. File email dev được lưu ở đâu

- `SaveDevelopmentCopyAsync(...)`: dòng 72-110
- Thư mục:
  - `App_Data/DevEmails`, dòng 82-83

## 11. Background job báo cáo tuần

Nguồn đọc: `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs`

- `ExecuteAsync(...)`: dòng 19-34
- `SendWeeklyReportsIfNeededAsync(...)`: dòng 36-83

Luồng:

1. Worker chạy vòng lặp
2. Cứ mỗi 30 phút kiểm tra 1 lần:
   - `Task.Delay(TimeSpan.FromMinutes(30))` ở dòng 32
3. Chỉ chạy khi:
   - là thứ Hai
   - giờ từ 8h đến 9h
   - xem dòng 39-42
4. Load user active + verified + role `User`:
   - dòng 51-54
5. Check đã gửi báo cáo tuần kỳ này chưa:
   - dòng 58-61
6. Nếu chưa, build email và queue email:
   - dòng 68-74
7. Ghi `ReportDispatchLog`:
   - dòng 76-81

Điểm kiến trúc đẹp:

- Có `ReportDispatchLog` nên không gửi lặp cùng một kỳ

## 12. Realtime bằng SignalR

Nguồn đọc:

- `DoAnLTW.Web/Hubs/BudgetHub.cs`
- `DoAnLTW.Web/wwwroot/js/site.js`
- `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs`
- `DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs`

### 12.1. Hub server

- `BudgetHub.OnConnectedAsync(...)`: dòng 9-19
- Khi client kết nối, server add connection vào group `user-{userId}`

### 12.2. Server bắn event

- Budget alert:
  - `BudgetMonitorService.cs:90-98`
  - event name: `budgetAlert`
- Wallet alert:
  - `WalletBalanceMonitorService.cs:36-43`
  - event name: `walletAlert`

### 12.3. Client nhận event

Nguồn đọc: `DoAnLTW.Web/wwwroot/js/site.js`

- `showRealtimeToast(...)`: dòng 1-22
- Khởi tạo SignalR sau `DOMContentLoaded`: dòng 24-84
- Tạo connection ở dòng 69-72
- Nhận `budgetAlert` ở dòng 74-77
- Nhận `walletAlert` ở dòng 79-82

## 13. Audit log và log hệ thống

Nguồn đọc:

- `DoAnLTW.Web/Services/Finance/AuditLogService.cs`
- `DoAnLTW.Web/Controllers/AdminController.cs`

### Service ghi log

- `WriteAsync(...)`: dòng 15-32
- Thêm `SystemLog` rồi `SaveChangesAsync`

### Những nơi hay gọi log

- `AuthController` khi đăng ký, reset password
- `TransactionsController` khi tạo/sửa/xóa giao dịch
- `WalletsController` khi tạo/sửa/xóa ví
- `BudgetsController` khi lưu/xóa ngân sách
- `ProfileController` khi cập nhật hồ sơ
- `AdminController` khi admin thao tác user/category

## 14. Công thức và thuật toán quan trọng của dự án

Đây là phần nên học thuộc nếu bạn sắp thuyết trình hoặc bảo vệ đồ án.

| Chủ đề | Công thức / thuật toán | Nằm ở đâu |
|---|---|---|
| Hash mật khẩu | `PBKDF2 + SHA256 + salt + 100000 iterations` | `PasswordService.cs:11-36` |
| OTP | Sinh số ngẫu nhiên 6 chữ số, lưu `SHA256(code)`, hết hạn 10 phút, tối đa 5 lần sai | `OtpService.cs:21-100` |
| Gợi ý danh mục | Normalize note -> tách keyword -> ưu tiên keyword cá nhân -> fallback rule hệ thống | `CategorizationService.cs:55-107` |
| Học từ user | Nếu user chọn khác gợi ý thì lưu tối đa 5 keyword vào `UserPersonalKeyword` | `CategorizationService.cs:109-157` |
| Số dư ví | `Expense => balance - amount`, `Income => balance + amount` | `TransactionsController.cs:374-396` |
| % sử dụng ngân sách | `spent / limit * 100` | `BudgetsController.cs:170`, `DashboardController.cs:245`, `BudgetMonitorService.cs:56` |
| Tỷ lệ tiết kiệm | `(income - expense) / income * 100` | `DashboardController.cs:131` |
| Dự báo chi cuối tháng | `0.65 * currentForecast + 0.35 * averagePastMonths` | `ForecastService.cs:65` |
| Spike detection | `threshold = max(Q3 + 1.5*IQR, median*3, 500000)` | `ForecastService.cs:68-91` |
| Chênh lệch báo cáo | `BalanceChange = totalIncome - totalExpense` | `ReportService.cs:38` |
| Chi TB/ngày | `AverageExpensePerDay = totalExpense / totalDays` | `ReportService.cs:40` |
| Net tháng dashboard | `NetAmount = Income - Expense` | `MonthlyComparisonItem.cs:13` |
| Pie chart dashboard | Tính % từng category rồi dựng `conic-gradient(...)` | `Views/Dashboard/Index.cshtml:34-63` |

## 15. File nào chứa chức năng nào

| Chức năng | File nên mở đầu tiên |
|---|---|
| Khởi động app, DI, middleware | `DoAnLTW.Web/Program.cs` |
| Schema DB, index, quan hệ | `DoAnLTW.Web/Data/FinanceDbContext.cs` |
| Đăng ký, login, OTP | `DoAnLTW.Web/Controllers/AuthController.cs` |
| Hash password | `DoAnLTW.Web/Services/Security/PasswordService.cs` |
| Gửi/verify OTP | `DoAnLTW.Web/Services/Auth/OtpService.cs` |
| Tạo cookie auth | `DoAnLTW.Web/Services/Auth/CookieAuthService.cs` |
| Tạo/sửa/xóa giao dịch | `DoAnLTW.Web/Controllers/TransactionsController.cs` |
| Gợi ý category từ note | `DoAnLTW.Web/Services/Finance/CategorizationService.cs` |
| Quản lý ví | `DoAnLTW.Web/Controllers/WalletsController.cs` |
| Quản lý ngân sách | `DoAnLTW.Web/Controllers/BudgetsController.cs` |
| Dashboard | `DoAnLTW.Web/Controllers/DashboardController.cs` |
| Dự báo chi tiêu | `DoAnLTW.Web/Services/Finance/ForecastService.cs` |
| Cảnh báo ngân sách | `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs` |
| Cảnh báo ví thấp | `DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs` |
| Báo cáo, Excel, email report | `DoAnLTW.Web/Services/Finance/ReportService.cs` |
| Email queue/background | `DoAnLTW.Web/Services/Email/*` |
| Báo cáo tuần tự động | `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs` |
| Realtime SignalR | `DoAnLTW.Web/Hubs/BudgetHub.cs`, `DoAnLTW.Web/wwwroot/js/site.js` |
| Admin | `DoAnLTW.Web/Controllers/AdminController.cs` |
| Seed dữ liệu | `DoAnLTW.Web/Data/SeedData.cs`, `DoAnLTW.Web/Data/Seeders/*` |

## 16. Thứ tự đọc code nếu bạn muốn hiểu sâu

### Cách 1: đọc theo luồng hệ thống

1. `Program.cs`
2. `FinanceDbContext.cs`
3. `IdentityEntities.cs`
4. `FinanceEntities.cs`
5. `SystemEntities.cs`
6. `SeedData.cs`
7. `ReferenceDataSeeder.cs`
8. `AuthController.cs`
9. `PasswordService.cs`
10. `OtpService.cs`
11. `CookieAuthService.cs`
12. `TransactionsController.cs`
13. `CategorizationService.cs`
14. `BudgetMonitorService.cs`
15. `WalletBalanceMonitorService.cs`
16. `DashboardController.cs`
17. `ForecastService.cs`
18. `ReportService.cs`
19. `QueuedEmailBackgroundService.cs`
20. `SmtpEmailSender.cs`
21. `WeeklyReportHostedService.cs`

### Cách 2: đọc theo luồng người dùng

1. Đăng ký và OTP
   - `AuthController.cs`
   - `OtpService.cs`
2. Đăng nhập
   - `AuthController.cs`
   - `CookieAuthService.cs`
3. Tạo ví
   - `WalletsController.cs`
4. Nhập giao dịch
   - `TransactionsController.cs`
   - `CategorizationService.cs`
5. Nhận cảnh báo budget / ví
   - `BudgetMonitorService.cs`
   - `WalletBalanceMonitorService.cs`
   - `BudgetHub.cs`
   - `site.js`
6. Xem dashboard
   - `DashboardController.cs`
   - `Views/Dashboard/Index.cshtml`
7. Xem báo cáo / Excel / email
   - `ReportsController.cs`
   - `ReportService.cs`

## 17. Nhận xét kiến trúc để bạn dễ nói chuyện với giảng viên

### Điểm tốt

- Tách service khá rõ: auth, finance, email
- Dùng EF Core + DbContext thống nhất
- Có background service cho email/report
- Có realtime SignalR cho cảnh báo
- Có seed dữ liệu demo khá đầy đủ
- Có audit log và admin area

### Điểm bạn có thể nói là hướng cải tiến

- Một phần business logic còn nằm trong controller, có thể tách thêm sang service
- Một số đoạn tính `spent` trong vòng lặp có thể tối ưu query nếu dữ liệu lớn
- Rule gợi ý category hiện là heuristic/rule-based, có thể nâng cấp sang ML/NLP sau

## 18. Kết luận ngắn gọn

Nếu phải tóm tắt dự án trong 1 đoạn:

Đây là web app quản lý chi tiêu cá nhân theo kiến trúc ASP.NET Core MVC. Người dùng đăng nhập bằng cookie auth, dữ liệu lưu ở SQL Server qua EF Core, giao dịch làm thay đổi số dư ví và kích hoạt kiểm tra ngân sách. Hệ thống có phần gợi ý danh mục dựa trên từ khóa và lịch sử chọn của user, có dashboard tổng hợp thu chi và dự báo chi cuối tháng, có báo cáo Excel/email, và có SignalR để đẩy cảnh báo ngân sách hoặc ví sắp hết tiền theo thời gian thực.

---

Nếu bạn muốn, bước tiếp theo mình có thể làm thêm 1 file nữa tên kiểu `LUONG_XU_LY_THEO_SO_DO.md` để vẽ lại các flow quan trọng theo dạng mũi tên:

- Đăng ký -> OTP -> xác thực -> đăng nhập
- Tạo giao dịch -> cập nhật ví -> check budget -> SignalR/email
- Xem dashboard -> query -> group/sum -> render chart
