# Giải Phẫu Codebase DoAnLTW

Tài liệu này giải thích code theo hướng “đọc để hiểu và bảo vệ đồ án”, không chỉ liệt kê tên file. Mục tiêu là làm rõ:

- Ứng dụng khởi động từ đâu.
- DI được đăng ký ở đâu, vì sao chọn lifetime như vậy.
- `async/await` đang được dùng ở đâu và để làm gì.
- Mỗi `class`, `service`, `controller`, `entity`, `view model` giữ vai trò gì.
- Các công thức nghiệp vụ nằm ở đâu.
- Những điểm dễ bị hỏi khi demo hoặc bảo vệ.

Tài liệu này ưu tiên phần backend vì đây là nơi chứa gần như toàn bộ nghiệp vụ của hệ thống.

## 1. Bức tranh tổng thể

Đây là ứng dụng quản lý chi tiêu cá nhân viết bằng ASP.NET Core MVC trên .NET 8.

Thành phần chính:

- `Program.cs`: điểm vào của ứng dụng, cấu hình dịch vụ, middleware, route, SignalR, background service.
- `FinanceDbContext`: cấu hình EF Core, ánh xạ bảng và quan hệ.
- `Models/Entities`: mô tả cấu trúc dữ liệu lưu trong SQL Server.
- `Controllers`: nhận request, kiểm tra dữ liệu, gọi service, trả về View hoặc File/Json.
- `Services`: chứa logic nghiệp vụ như xác thực, OTP, dự báo, báo cáo, cảnh báo ngân sách, email.
- `Views`: Razor View để hiển thị giao diện.
- `wwwroot`: CSS, JS, ảnh và thư viện frontend.

Luồng điển hình khi người dùng tạo một giao dịch chi:

1. Trình duyệt gửi `POST` lên `TransactionsController.Save`.
2. ASP.NET Core model binding đưa dữ liệu vào `TransactionFormViewModel`.
3. Controller kiểm tra `ModelState`, ví, danh mục và loại giao dịch.
4. `CategorizationService` gợi ý danh mục từ ghi chú.
5. Controller cập nhật `Wallet.CurrentBalance`.
6. EF Core lưu `WalletTransaction` vào database.
7. `CategorizationService.LearnAsync` học thêm từ khóa theo hành vi người dùng.
8. `BudgetMonitorService` tính mức dùng ngân sách và tạo cảnh báo nếu vượt hạn mức.
9. `WalletBalanceMonitorService` phát cảnh báo realtime nếu ví vừa rơi xuống dưới ngưỡng thấp.
10. `AuditLogService` ghi log hành động.
11. Controller redirect về trang danh sách và UI đọc `TempData` để hiển thị thông báo.

## 2. Ứng dụng khởi động như thế nào

Toàn bộ quá trình startup nằm trong `DoAnLTW.Web/Program.cs`.

### 2.1. Tạo host và đọc cấu hình

- `WebApplication.CreateBuilder(args)` tạo `builder` chứa configuration, logging, service collection, environment.
- `GetConnectionString("DefaultConnection")` đọc chuỗi kết nối SQL Server.
- `GetValue<bool>("SeedDemoData")` và `SeedShowcaseUserEmail` quyết định có seed dữ liệu mẫu hay không.
- Khối `if (string.IsNullOrWhiteSpace(defaultConnection)) throw ...` dùng để fail fast. Nếu chưa cấu hình database thì ứng dụng dừng ngay khi startup.

### 2.2. Logging

- `ClearProviders()` xóa provider mặc định.
- `AddConsole()` và `AddDebug()` giữ lại logging cần thiết cho môi trường phát triển.

### 2.3. Đăng ký framework nền

- `AddControllersWithViews()` bật mô hình MVC.
- `AddSignalR()` bật hạ tầng realtime.
- `AddHttpContextAccessor()` cho phép các service có thể truy cập `HttpContext` nếu cần.

### 2.4. Data Protection

Đoạn này tạo thư mục `App_Data/DataProtectionKeys` rồi gọi:

- `AddDataProtection()`
- `SetApplicationName("DoAnLTW")`
- `PersistKeysToFileSystem(...)`

Ý nghĩa:

- Cookie xác thực và một số dữ liệu được bảo vệ sẽ dùng key ring ổn định.
- Khi restart ứng dụng, cookie cũ vẫn có thể đọc được.
- Nếu không persist key, mỗi lần restart có thể làm người dùng bị đăng xuất.

### 2.5. Bind cấu hình vào class option

- `Configure<SmtpOptions>(...)` bind section `Smtp`.
- `Configure<AdminSeedOptions>(...)` bind section `AdminAccount`.

Mẫu này giúp service dùng `IOptions<T>` thay vì đọc thẳng từ `IConfiguration`.

### 2.6. Đăng ký database

- `AddDbContext<FinanceDbContext>(options => options.UseSqlServer(defaultConnection))`

Điểm quan trọng:

- `FinanceDbContext` mặc định có lifetime là `Scoped`.
- Mỗi HTTP request thường nhận một `DbContext` riêng.
- Điều này phù hợp với change tracking và transaction theo request.

### 2.7. Authentication và Authorization

Ứng dụng dùng Cookie Authentication:

- `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)`
- `AddCookie(...)`

Cấu hình đáng chú ý:

- `LoginPath = "/Auth/Login"`
- `AccessDeniedPath = "/Auth/AccessDenied"`
- `SlidingExpiration = true`
- `ExpireTimeSpan = 14 ngày`

Sau đó gọi `AddAuthorization()` để sử dụng `[Authorize]` và role-based authorization.

### 2.8. Đăng ký service nghiệp vụ

Các service được đăng ký trong DI container ngay tại `Program.cs`.

| Service | Lifetime | Lý do |
|---|---|---|
| `FinanceDbContext` | `Scoped` | Làm việc với DB theo từng request. |
| `PasswordService` | `Scoped` | Không giữ state, phục vụ logic bảo mật trong request. |
| `CookieAuthService` | `Scoped` | Xử lý đăng nhập/đăng xuất theo request. |
| `OtpService` | `Scoped` | Phụ thuộc `FinanceDbContext`, cần đồng lifetime với DB. |
| `CategorizationService` | `Scoped` | Đọc/ghi dữ liệu gợi ý cá nhân theo request. |
| `ForecastService` | `Scoped` | Tính dự báo dựa trên dữ liệu người dùng trong DB. |
| `BudgetMonitorService` | `Scoped` | Phụ thuộc DB, SignalR, email queue. |
| `WalletBalanceMonitorService` | `Scoped` | Kiểm tra cảnh báo số dư thấp. |
| `ReportService` | `Scoped` | Tổng hợp báo cáo, xuất Excel, tạo HTML email. |
| `AuditLogService` | `Scoped` | Ghi log vào DB. |
| `AvatarStorageService` | `Scoped` | Lưu ảnh đại diện theo request upload. |
| `IEmailSender -> SmtpEmailSender` | `Scoped` | Đóng gói logic gửi email. |
| `EmailQueue` | `Singleton` | Queue dùng chung cho toàn ứng dụng. |
| `QueuedEmailBackgroundService` | Hosted service | Worker gửi email nền. |
| `WeeklyReportHostedService` | Hosted service | Worker gửi báo cáo tuần tự động. |

### 2.9. Build app và pipeline middleware

Sau khi đăng ký service:

- `var app = builder.Build();`
- `await SeedData.InitializeAsync(...)` chạy migrate và seed dữ liệu.

Pipeline chính:

- `UseExceptionHandler(...)`
- `UseHsts()`
- `UseHttpsRedirection()`
- `UseStatusCodePagesWithReExecute(...)`
- `UseStaticFiles()`
- `UseRouting()`
- `UseAuthentication()`
- `UseAuthorization()`
- `MapHub<BudgetHub>("/budgetHub")`
- `MapControllerRoute(...)`

`app.Run()` là điểm ứng dụng bắt đầu lắng nghe request.

## 3. DI trong dự án này diễn ra ra sao

### 3.1. Constructor injection

Controller và service đều nhận dependency qua constructor.

Ví dụ:

- `AuthController` nhận `FinanceDbContext`, `PasswordService`, `OtpService`, `CookieAuthService`, `AuditLogService`.
- `TransactionsController` nhận `FinanceDbContext`, `CategorizationService`, `BudgetMonitorService`, `WalletBalanceMonitorService`, `AuditLogService`.
- `BudgetMonitorService` nhận `FinanceDbContext`, `IHubContext<BudgetHub>`, `EmailQueue`.

Ý nghĩa:

- Không cần tự tạo object bằng `new` ở mỗi nơi.
- Dễ test hơn.
- Dễ thay thế implementation, ví dụ `IEmailSender`.

### 3.2. Vì sao `EmailQueue` là `Singleton`

`EmailQueue` là hàng đợi trong memory. Nếu request A đưa email vào queue thì background worker phải đọc đúng queue đó.

Nếu queue là `Scoped` hoặc `Transient`:

- request và worker sẽ không nhìn thấy cùng một queue,
- email có thể bị “mất” trong luồng xử lý.

Vì vậy `Singleton` là hợp lý cho hàng đợi dùng chung toàn app.

### 3.3. Vì sao background service dùng `IServiceProvider`

`QueuedEmailBackgroundService` và `WeeklyReportHostedService` không inject thẳng `FinanceDbContext` vào constructor.

Thay vào đó, chúng inject `IServiceProvider`, rồi mỗi lần cần mới:

- `CreateScope()`
- resolve `FinanceDbContext`, `ReportService`, `IEmailSender`, ...

Lý do:

- Hosted service sống suốt vòng đời ứng dụng.
- `FinanceDbContext` lại là `Scoped`.
- Nếu inject thẳng một `Scoped` vào `HostedService`, lifetime sẽ sai.

## 4. `async/await` và xử lý bất đồng bộ

### 4.1. Dự án đang async ở đâu

`async/await` xuất hiện nhiều ở:

- EF Core: `ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`, `CountAsync`, `SumAsync`, `MigrateAsync`
- SignalR: `SendAsync`, `AddToGroupAsync`
- SMTP: `SendMailAsync`
- File I/O: `CopyToAsync`, `WriteAllTextAsync`
- Queue: `Channel.WriteAsync`, `Channel.ReadAsync`
- Background service: `ExecuteAsync`, `Task.Delay`

### 4.2. Vì sao phải dùng bất đồng bộ

Các thao tác trên chủ yếu là I/O-bound:

- chờ SQL Server trả kết quả,
- chờ ghi file,
- chờ SMTP server,
- chờ client SignalR.

Nếu viết đồng bộ:

- thread request sẽ bị block,
- số lượng request phục vụ đồng thời sẽ giảm,
- trải nghiệm người dùng dễ chậm hơn.

### 4.3. “Xử lý bất đối xứng” có ở đâu không

Nếu ý của bạn là mã hóa bất đối xứng kiểu RSA, ECC, public key/private key thì:

- Dự án hiện tại **không dùng mã hóa bất đối xứng**.

Phần bảo mật đang dùng:

- PBKDF2 để băm mật khẩu,
- SHA-256 để băm OTP,
- ASP.NET Core Data Protection để quản lý key cho cookie và dữ liệu cần bảo vệ.

## 5. Bảo mật và xác thực

### 5.1. `PasswordService`

File: `DoAnLTW.Web/Services/Security/PasswordService.cs`

Vai trò:

- băm mật khẩu trước khi lưu DB,
- kiểm tra mật khẩu khi đăng nhập.

Hằng số:

- `SaltSize = 16`
- `HashSize = 32`
- `Iterations = 100_000`

Công thức lưu mật khẩu:

1. Sinh `salt` ngẫu nhiên 16 byte.
2. Chạy `PBKDF2(password, salt, 100000, SHA256, 32)`.
3. Ghép thành chuỗi:
   - `v1.<iterations>.<base64(salt)>.<base64(hash)>`

Khi kiểm tra:

1. Tách chuỗi đã lưu.
2. Lấy lại `iterations`, `salt`, `expectedHash`.
3. Tính lại hash từ password người dùng vừa nhập.
4. So sánh bằng `CryptographicOperations.FixedTimeEquals`.

Ý nghĩa:

- Không lưu plain text password.
- Cùng một mật khẩu nhưng khác salt sẽ ra hash khác.
- So sánh constant-time giúp giảm rủi ro timing attack.

### 5.2. `OtpService`

File: `DoAnLTW.Web/Services/Auth/OtpService.cs`

Vai trò:

- tạo OTP,
- lưu OTP đã băm,
- đẩy email OTP vào queue,
- kiểm tra OTP khi xác thực.

Luồng `SendOtpAsync`:

1. Sinh OTP 6 chữ số bằng `RandomNumberGenerator.GetInt32(100000, 999999)`.
2. Băm OTP bằng SHA-256.
3. Lưu vào bảng `EmailOtps` với `ExpiresAt = UtcNow + 10 phút`.
4. Tạo `EmailMessage`.
5. Đưa vào `EmailQueue`.

Luồng `VerifyAsync`:

1. Chuẩn hóa email về lower-case.
2. Lấy OTP mới nhất chưa dùng theo `Email + Purpose + !IsUsed`.
3. Kiểm tra hết hạn.
4. Tăng `Attempts`.
5. Nếu quá 5 lần thì từ chối.
6. Băm lại mã user nhập để so với `CodeHash`.
7. Nếu đúng thì đánh dấu `IsUsed = true`.

Tại sao OTP không lưu raw:

- nếu database bị lộ, mã OTP gốc vẫn không lộ trực tiếp,
- tư duy này giống bảo vệ password, dù OTP có vòng đời ngắn hơn.

### 5.3. `CookieAuthService`

File: `DoAnLTW.Web/Services/Auth/CookieAuthService.cs`

Vai trò:

- tạo cookie đăng nhập,
- xóa cookie khi logout.

Khi đăng nhập thành công, service tạo các claim:

- `NameIdentifier = user.Id`
- `Name = user.Username`
- `GivenName = user.DisplayName`
- `Email = user.Email`
- `Role = user.Role.Name`

Sau đó:

- tạo `ClaimsIdentity`,
- tạo `ClaimsPrincipal`,
- gọi `HttpContext.SignInAsync(...)`.

`rememberMe` ảnh hưởng tới:

- `IsPersistent`
- `ExpiresUtc`

### 5.4. `ClaimsPrincipalExtensions`

File: `DoAnLTW.Web/Extensions/ClaimsPrincipalExtensions.cs`

Vai trò:

- đọc `UserId` từ claim `NameIdentifier`,
- đọc tên hiển thị từ claim `GivenName`.

Controller base dùng extension này để viết ngắn hơn:

- `CurrentUserId => User.GetUserId() ?? 0`

## 6. Database, entity và quan hệ

### 6.1. `FinanceDbContext`

File: `DoAnLTW.Web/Data/FinanceDbContext.cs`

`DbSet` chính:

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

Index quan trọng:

- `Role.Name` unique
- `AppUser.Username` unique
- `AppUser.Email` unique
- `Budget(UserId, CategoryId, Year, Month)` unique
- `UserPersonalKeyword(UserId, Keyword)` unique
- `ReportDispatchLog(UserId, ReportType, PeriodKey)` unique

Delete behavior đáng chú ý:

- `Role -> Users`: `Restrict`
- `User -> Wallets`: `Cascade`
- `WalletTransaction -> Wallet`: `Restrict`
- `BudgetAlert -> Budget`: `NoAction`
- `SystemLog -> User`: `SetNull`

### 6.2. Nhóm entity xác thực

#### `Role`

- lưu vai trò như `Admin`, `User`
- một role có nhiều user

#### `AppUser`

Thuộc tính quan trọng:

- `Username`
- `DisplayName`
- `Email`
- `PasswordHash`
- `AvatarPath`
- `IsActive`
- `IsEmailVerified`
- `PhoneNumber`
- `CreatedAt`
- `RoleId`

Navigation:

- `Wallets`
- `Transactions`
- `Budgets`
- `PersonalKeywords`
- `Alerts`

#### `EmailOtp`

- lưu mã OTP đã băm,
- có `Purpose` để phân biệt đăng ký và quên mật khẩu,
- có `Attempts` để giới hạn số lần thử,
- có `IsUsed` để tránh dùng lại.

### 6.3. Nhóm entity tài chính

#### `Wallet`

Vai trò:

- đại diện cho một ví hoặc tài khoản tiền.

Thuộc tính đáng chú ý:

- `InitialBalance`
- `CurrentBalance`
- `IsArchived`

Điểm quan trọng:

- `CurrentBalance` được lưu sẵn trong DB,
- không phải mỗi lần đều sum lại toàn bộ transaction.

#### `Category`

Vai trò:

- phân loại giao dịch thành nhóm chi hoặc thu.

Thuộc tính chính:

- `Name`
- `TransactionType`
- `Icon`
- `ColorHex`
- `IsDefault`
- `CreatedByUserId`

#### `WalletTransaction`

Vai trò:

- bản ghi một lần thu hoặc chi.

Thuộc tính đáng chú ý:

- `Type`: `Expense` hoặc `Income`
- `Amount`
- `Note`
- `OccurredOn`
- `CreatedAt`
- `AiSuggestedCategoryId`
- `AiSuggestedLabel`
- `AiConfidence`

#### `Budget`

Vai trò:

- hạn mức chi theo danh mục và theo tháng.

Thuộc tính chính:

- `CategoryId`
- `Year`
- `Month`
- `LimitAmount`
- `AlertThresholdPercent`

Lưu ý quan trọng:

- `AlertThresholdPercent` đã tồn tại trong model và UI,
- nhưng `BudgetMonitorService` hiện chỉ tạo cảnh báo khi `UsagePercent >= 100`.

#### `UserPersonalKeyword`

Vai trò:

- lưu “trí nhớ cá nhân” của người dùng cho việc gợi ý danh mục.

Thuộc tính:

- `Keyword`
- `CategoryId`
- `HitCount`
- `LastUsedAt`

#### `BudgetAlert`

Vai trò:

- lưu lịch sử cảnh báo vượt ngân sách.

Thuộc tính quan trọng:

- `Message`
- `SpentAmount`
- `LimitAmount`
- `UsagePercent`
- `IsRead`
- `IsEmailSent`

### 6.4. Nhóm entity hệ thống

#### `SystemLog`

Vai trò:

- audit log và log nghiệp vụ.

Thuộc tính:

- `Level`
- `Action`
- `Message`
- `Data`
- `CreatedAt`
- `UserId`

#### `ReportDispatchLog`

Vai trò:

- theo dõi lịch sử gửi báo cáo.

Thuộc tính:

- `ReportType`
- `PeriodKey`
- `SentAt`

## 7. Service nghiệp vụ

### 7.1. `CategorizationService`

File: `DoAnLTW.Web/Services/Finance/CategorizationService.cs`

Đây là service gợi ý danh mục cho giao dịch.

Nó không dùng AI model bên ngoài mà dùng:

- rule cố định,
- trí nhớ cá nhân học từ người dùng.

Thành phần:

- `ExpenseRules`: từ khóa -> tên danh mục chi
- `IncomeRules`: từ khóa -> tên danh mục thu
- `Normalize`: bỏ dấu, lowercase, chuẩn hóa khoảng trắng
- `ExtractKeywords`: tách từ khóa
- `RemoveDiacritics`: bỏ dấu tiếng Việt

Luồng `SuggestAsync`:

1. Nếu ghi chú rỗng thì trả kết quả rỗng.
2. Chuẩn hóa text.
3. Tách keyword.
4. Lấy `UserPersonalKeywords` của người dùng theo loại giao dịch.
5. Nếu keyword nào khớp dữ liệu cá nhân thì trả về nguồn `personal`, độ tin cậy `0.98`.
6. Nếu không có, lấy danh sách category theo loại giao dịch.
7. Duyệt qua bộ rule.
8. Nếu ghi chú chứa từ khóa của rule và tìm được category tương ứng thì trả nguồn `rule`, độ tin cậy `0.75`.

Luồng `LearnAsync`:

1. Nếu note rỗng thì bỏ qua.
2. Nếu user chọn đúng y hệt suggestion thì bỏ qua.
3. Nếu user chọn category khác suggestion thì lấy tối đa 5 keyword đầu tiên.
4. Cập nhật hoặc thêm `UserPersonalKeyword`.

### 7.2. `ForecastService`

File: `DoAnLTW.Web/Services/Finance/ForecastService.cs`

Đây là service chứa công thức dự báo chi tiêu cuối tháng.

Mục tiêu:

- dự báo tổng chi tới cuối tháng,
- ưu tiên dữ liệu tháng hiện tại,
- vẫn tham chiếu lịch sử 3 tháng gần nhất,
- tách khoản chi đột biến ra khỏi phần chi đều.

Luồng `CalculateMonthEndExpenseForecastAsync`:

1. Xác định đầu tháng và cuối tháng.
2. Chặn `today` vào trong phạm vi tháng đang tính.
3. Lấy ngày tạo tài khoản.
4. Tính `observationStart`.
5. Tính số ngày đã quan sát.
6. Lấy tất cả khoản chi của tháng hiện tại.
7. Nếu chưa có dữ liệu thì dùng trung bình 3 tháng trước.
8. Tính ngưỡng chi đột biến.
9. Tách dữ liệu thành:
   - `spikeTotal`
   - `routineTotal`
10. Nội suy phần chi thường xuyên cho cả tháng.
11. Trộn với trung bình các tháng trước theo trọng số `65/35`.

Ngưỡng đột biến:

- `IQR = Q3 - Q1`
- `IqrThreshold = Q3 + 1.5 * IQR`
- `MinThreshold = Median * 3`
- `SpikeThreshold = max(IqrThreshold, MinThreshold, 500000)`

### 7.3. `BudgetMonitorService`

File: `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs`

Vai trò:

- kiểm tra ngân sách,
- tạo `BudgetAlert`,
- gửi SignalR realtime,
- queue email cảnh báo.

Luồng `CheckAndNotifyAsync`:

1. Nạp transaction theo `transactionId`.
2. Nếu không phải `Expense` thì dừng.
3. Xác định tháng/năm từ `OccurredOn`.
4. Tìm `Budget` cùng user, category, tháng, năm.
5. Nếu không có budget thì dừng.
6. Tính `spentAmount` bằng tổng chi cùng category trong tháng đó.
7. Tính `usagePercent = spentAmount / limitAmount * 100`.
8. Nếu chưa đạt 100% thì chưa tạo alert.
9. Kiểm tra alert đã tồn tại cho đúng transaction đó hay chưa.
10. Tạo `BudgetAlert`.
11. Lưu DB.
12. Gửi event `budgetAlert` qua SignalR.
13. Gọi `TrySendBudgetAlertEmailAsync`.

`CheckBudgetStatusAsync` được dùng khi:

- user vừa tạo hoặc sửa budget,
- hệ thống cần kiểm tra xem tháng đó đã vượt limit hay chưa.

### 7.4. `WalletBalanceMonitorService`

File: `DoAnLTW.Web/Services/Finance/WalletBalanceMonitorService.cs`

Vai trò:

- phát hiện ví vừa rơi xuống dưới ngưỡng thấp.

Hằng số:

- `LowBalanceThreshold = 200000m`

Điều kiện gửi cảnh báo:

- trước đó số dư phải `>= threshold`
- hiện tại số dư phải `< threshold`

### 7.5. `ReportService`

File: `DoAnLTW.Web/Services/Finance/ReportService.cs`

Vai trò:

- tổng hợp báo cáo,
- xuất Excel,
- tạo email HTML cho báo cáo.

Luồng `BuildSummaryAsync`:

1. Quy đổi `to` sang `endExclusive = to.Date.AddDays(1)`.
2. Lấy transaction trong khoảng ngày.
3. Tính:
   - `TotalIncome`
   - `TotalExpense`
   - `BalanceChange`
   - `TransactionCount`
   - `AverageExpensePerDay`
4. Group top category chi tiêu.
5. Map transaction sang `ReportTransactionItem`.

`ExportExcelAsync`:

- gọi `BuildSummaryAsync`,
- dùng `ClosedXML`,
- ghi thông tin tổng hợp,
- ghi bảng giao dịch,
- trả về `byte[]`.

### 7.6. Email stack

#### `EmailMessage`

DTO gói email:

- `To`
- `Subject`
- `HtmlBody`

#### `IEmailSender`

Interface chuẩn hóa việc gửi email:

- `Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)`

#### `EmailQueue`

Vai trò:

- hàng đợi email trong memory, dùng `Channel<EmailMessage>`.

#### `QueuedEmailBackgroundService`

Vai trò:

- worker nền, lấy email từ queue để gửi.

Luồng:

1. Chờ `DequeueAsync`.
2. Tạo scope DI mới.
3. Resolve `IEmailSender`.
4. Gửi email.
5. Bắt lỗi và log nếu có.

#### `SmtpEmailSender`

Vai trò:

- gửi email thật qua SMTP,
- hoặc lưu bản email HTML khi môi trường development chưa có cấu hình SMTP chuẩn.

### 7.7. `AuditLogService`

Rất đơn giản:

- tạo `SystemLog`
- `SaveChangesAsync`

Nhưng đây là điểm tập trung để toàn bộ controller ghi log đồng nhất.

### 7.8. `AvatarStorageService`

Vai trò:

- lưu ảnh đại diện vào `wwwroot/uploads/avatars`,
- chỉ cho phép `.jpg`, `.jpeg`, `.png`, `.webp`,
- xóa ảnh cũ nếu upload ảnh mới.

## 8. Controller và luồng xử lý

### 8.1. `AppControllerBase`

Đây là lớp base cho controller cần đăng nhập.

Nó cung cấp:

- `CurrentUserId`
- `SetBudgetAlertWarning(...)`

### 8.2. `AuthController`

Vai trò:

- đăng nhập,
- đăng ký,
- xác thực OTP,
- quên mật khẩu,
- đặt lại mật khẩu,
- logout.

Điểm đáng chú ý:

- `Login(POST)` kiểm tra:
  - user tồn tại,
  - password đúng,
  - tài khoản còn active,
  - email đã xác thực OTP.
- `Register(POST)`:
  - kiểm tra trùng email/username,
  - hash password,
  - tạo user với `IsEmailVerified = false`,
  - gửi OTP.
- `VerifyOtp(POST)`:
  - nếu purpose là `Register` thì set `IsEmailVerified = true`.
- `ForgotPassword(POST)`:
  - nếu email tồn tại thì gửi OTP reset password,
  - nhưng phản hồi vẫn giữ trung tính để tránh lộ thông tin account có tồn tại hay không.

### 8.3. `TransactionsController`

Đây là controller nghiệp vụ phức tạp nhất.

#### `Save`

Khi tạo mới:

1. Validate model.
2. Kiểm tra category đúng loại.
3. Gọi `SuggestAsync`.
4. Tìm wallet hợp lệ của user.
5. Lưu `walletBalanceBefore`.
6. Tạo `WalletTransaction`.
7. Gọi `ApplyWalletBalance`.
8. Lưu DB.
9. `LearnAsync`.
10. `CheckAndNotifyAsync`.
11. `NotifyIfThresholdCrossedAsync`.
12. `WriteAsync` audit log.

Khi cập nhật:

1. Nạp transaction cũ và wallet cũ.
2. Xác định wallet đích.
3. `ReverseWalletBalance` để hoàn tác ảnh hưởng cũ.
4. Cập nhật field transaction.
5. `ApplyWalletBalance` để áp ảnh hưởng mới.
6. Xóa alert cũ của transaction.
7. Lưu DB.
8. Học lại gợi ý cá nhân.
9. Kiểm tra budget và low-balance.

Điểm cực kỳ quan trọng:

- số dư ví được lưu sẵn,
- nên khi sửa giao dịch phải “hoàn tác cũ rồi áp mới”.

### 8.4. `DashboardController`

Vai trò:

- hiển thị dashboard người dùng.

Nó tính:

- thu tháng,
- chi tháng,
- top danh mục chi,
- biểu đồ 7 ngày,
- budget highlight,
- forecast,
- cảnh báo chưa đọc.

### 8.5. `BudgetsController`

Vai trò:

- tạo, sửa, xóa ngân sách theo tháng.

Điểm đáng chú ý:

- kiểm tra unique theo `(UserId, CategoryId, Year, Month)`
- sau khi lưu budget sẽ gọi `CheckBudgetStatusAsync`

### 8.6. `ReportsController`

Vai trò:

- xem báo cáo tuần,
- xem báo cáo tháng,
- xem báo cáo theo khoảng ngày tùy chọn,
- xuất Excel,
- gửi email báo cáo.

### 8.7. `WalletsController`

Vai trò:

- tạo và cập nhật ví,
- tính tổng số dư,
- chặn xóa ví nếu đã có giao dịch.

Điểm quan trọng nhất:

- khi sửa `InitialBalance`, controller không overwrite `CurrentBalance`,
- mà tính:
  - `initialBalanceDelta = InitialBalanceMới - InitialBalanceCũ`
  - `CurrentBalance += initialBalanceDelta`

### 8.8. `ProfileController`

Vai trò:

- cập nhật tên hiển thị,
- đổi email,
- upload avatar,
- đổi mật khẩu.

Khi đổi mật khẩu:

- phải verify `CurrentPassword`,
- nếu đúng mới hash và lưu `NewPassword`.

### 8.9. `AdminController`

Vai trò:

- dashboard quản trị,
- quản lý user,
- quản lý category,
- xem log hệ thống.

Điểm nghiệp vụ đáng chú ý:

- không cho khóa tài khoản admin từ màn hình toggle user,
- không cho đổi `TransactionType` của category nếu category đó đã được dùng,
- không cho xóa category nếu đã gắn transaction hoặc budget.

### 8.10. `HomeController`

Controller rất đơn giản:

- `Index()`
- `ErrorStatus(int code)`

## 9. Realtime và background processing

### 9.1. `BudgetHub`

Vai trò:

- khi user kết nối SignalR, đưa connection vào group `user-{userId}`.

Nhờ vậy:

- server có thể gửi thông điệp đúng người nhận,
- không cần broadcast cho toàn bộ client.

### 9.2. `QueuedEmailBackgroundService`

Đây là worker theo mô hình producer-consumer.

Producer:

- `OtpService`
- `BudgetMonitorService`
- `ReportsController`
- `WeeklyReportHostedService`

Consumer:

- `QueuedEmailBackgroundService`

Ưu điểm:

- request web không phải chờ SMTP gửi xong,
- UI phản hồi nhanh hơn.

Hạn chế:

- queue hiện là in-memory,
- restart app có thể làm mất email chưa gửi.

### 9.3. `WeeklyReportHostedService`

Vai trò:

- cứ mỗi vòng lặp sẽ kiểm tra điều kiện gửi báo cáo tuần.

Điều kiện hiện tại:

- hôm nay là `Monday`
- giờ hiện tại nằm trong khoảng 8h đến 9h

Luồng:

1. Tính `weekStart`, `weekEnd`, `periodKey`.
2. Tạo scope DI.
3. Lấy user hợp lệ.
4. Kiểm tra `ReportDispatchLog` để tránh gửi trùng.
5. Tạo HTML báo cáo.
6. Queue email.
7. Ghi `ReportDispatchLog`.

## 10. Seeding dữ liệu

### 10.1. `SeedData.InitializeAsync`

Thứ tự:

1. Tạo scope.
2. `MigrateAsync()`
3. `ReferenceDataSeeder.SeedAsync(...)`
4. Nếu bật demo thì `DemoFinanceDataSeeder.SeedAsync(...)`
5. Nếu có `SeedShowcaseUserEmail` thì seed dữ liệu showcase cho user đó.

### 10.2. `ReferenceDataSeeder`

Vai trò:

- tạo role `Admin`, `User`
- đồng bộ category mặc định
- tạo hoặc cập nhật tài khoản admin mặc định

### 10.3. `DemoFinanceDataSeeder`

File này dài vì không chỉ seed bảng đơn giản, mà seed cả bộ dữ liệu trình diễn:

- user mẫu,
- ví mẫu,
- giao dịch mẫu,
- ngân sách mẫu,
- alert mẫu,
- log mẫu,
- dữ liệu kéo dài nhiều tháng để demo dashboard, report và forecast.

## 11. ViewModel và dữ liệu ra vào UI

ViewModel được dùng để:

- tách dữ liệu UI ra khỏi entity DB,
- kiểm tra dữ liệu nhập bằng `DataAnnotations`,
- gom dữ liệu của nhiều phần thành một model cho view.

Nhóm chính:

- Auth:
  - `LoginViewModel`
  - `RegisterViewModel`
  - `VerifyOtpViewModel`
  - `ForgotPasswordViewModel`
  - `ResetPasswordViewModel`
- Wallet/Budget/Transaction:
  - `WalletFormViewModel`
  - `WalletItemViewModel`
  - `WalletsPageViewModel`
  - `BudgetFormViewModel`
  - `BudgetProgressViewModel`
  - `BudgetsPageViewModel`
  - `TransactionFormViewModel`
  - `TransactionFilterViewModel`
  - `TransactionHistoryItemViewModel`
  - `TransactionsPageViewModel`
- Dashboard/Report/Admin:
  - `DashboardViewModel`
  - `WalletMiniCardViewModel`
  - `MonthlyComparisonItem`
  - `ReportFilterViewModel`
  - `ReportSummaryViewModel`
  - `ReportTransactionItem`
  - `ReportsPageViewModel`
  - `AdminDashboardPageViewModel`
  - `AdminUserItemViewModel`
  - `AdminUsersPageViewModel`
  - `AdminCategoryItemViewModel`
  - `AdminCategoryFormViewModel`
  - `AdminCategoriesPageViewModel`
  - `AdminDailyStatViewModel`
  - `SystemLogRowViewModel`
  - `AdminLogsPageViewModel`

## 12. Công thức và quy tắc nghiệp vụ quan trọng

### 12.1. Số dư ví

Tạo giao dịch chi:

- `CurrentBalance = CurrentBalance - Amount`

Tạo giao dịch thu:

- `CurrentBalance = CurrentBalance + Amount`

Xóa giao dịch chi:

- `CurrentBalance = CurrentBalance + Amount`

Xóa giao dịch thu:

- `CurrentBalance = CurrentBalance - Amount`

Sửa số dư ban đầu của ví:

- `CurrentBalance = CurrentBalance + (InitialBalanceMới - InitialBalanceCũ)`

### 12.2. Mức dùng ngân sách

- `UsagePercent = SpentAmount / LimitAmount * 100`
- nếu `LimitAmount = 0` thì trả `0`

### 12.3. Tỷ lệ tiết kiệm

- `SavingsRate = ((TotalIncomeMonth - TotalExpenseMonth) / TotalIncomeMonth) * 100`
- nếu `TotalIncomeMonth = 0` thì trả `0`

### 12.4. Chi trung bình mỗi ngày

- `AverageExpensePerDay = round(TotalExpense / TotalDays)`

### 12.5. Dự báo chi cuối tháng

- `RoutineForecast = RoutineTotal / DaysPassed * DaysInMonth`
- `CurrentForecast = SpikeTotal + RoutineForecast`
- `FinalForecast = round(CurrentForecast * 0.65 + AveragePastMonths * 0.35)`

### 12.6. Ngưỡng chi đột biến

- `IQR = Q3 - Q1`
- `IqrThreshold = Q3 + 1.5 * IQR`
- `MinThreshold = Median * 3`
- `SpikeThreshold = max(IqrThreshold, MinThreshold, 500000)`

## 13. Những điểm rất dễ bị hỏi khi bảo vệ

- DI đăng ký ở đâu?
  - Trong `Program.cs` bằng `builder.Services...`
- Vì sao dùng `Scoped` cho đa số service?
  - Vì phần lớn service phụ thuộc `FinanceDbContext`.
- Vì sao `EmailQueue` là `Singleton`?
  - Vì request và worker cần nhìn thấy cùng một queue.
- Vì sao hosted service phải `CreateScope()`?
  - Vì không được inject thẳng `Scoped service` vào một service sống suốt vòng đời app.
- Dự án có mã hóa bất đối xứng không?
  - Không. Dự án hiện dùng PBKDF2, SHA-256 và Data Protection.
- Vì sao `CurrentBalance` không tính động mỗi lần?
  - Để đọc nhanh hơn, đổi lại phải cập nhật chính xác khi transaction thay đổi.
- Vì sao sửa transaction phải reverse rồi apply?
  - Vì phải hoàn tác ảnh hưởng cũ trước khi áp ảnh hưởng mới.
- Vì sao alert budget gửi email qua queue?
  - Để request web không bị chờ SMTP.
- `AlertThresholdPercent` đang dùng như thế nào?
  - Đã có trong model, nhưng logic hiện tại chưa dùng để cảnh báo sớm.

## 14. Hạn chế hiện tại nên nói trung thực

- `EmailQueue` là in-memory, restart app có thể làm mất email chưa gửi.
- `AlertThresholdPercent` chưa tham gia đầy đủ vào logic cảnh báo.
- `DashboardController` nạp khá nhiều transaction vào memory trước khi tính toán.
- Dự án không dùng repository pattern riêng, controller/service làm việc trực tiếp với `FinanceDbContext`.
- Không có mã hóa bất đối xứng.

## 15. Thứ tự đọc code để nắm nhanh nhất

Nếu chỉ có khoảng 15 đến 20 phút:

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
12. `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs`
13. `DoAnLTW.Web/Controllers/AdminController.cs`

## 16. Mẫu trả lời ngắn gọn khi bị hỏi

### 16.1. Về DI

"DI được đăng ký trong `Program.cs`. Các service nghiệp vụ chủ yếu để `Scoped` vì phụ thuộc `FinanceDbContext`. `EmailQueue` để `Singleton` vì request và background worker phải dùng chung một queue. Hai hosted service dùng `IServiceProvider.CreateScope()` để resolve service scoped đúng lifetime."

### 16.2. Về bất đồng bộ

"Em dùng `async/await` cho các tác vụ I/O như query SQL, ghi file, gửi SignalR và gửi SMTP. Mục tiêu là không block thread request. Email không gửi trực tiếp trong request mà được đưa vào queue rồi background worker xử lý."

### 16.3. Về bảo mật mật khẩu và OTP

"Mật khẩu được băm bằng PBKDF2 với salt riêng và 100000 vòng lặp. OTP 6 số được băm SHA-256 trước khi lưu vào DB, có thời hạn 10 phút và giới hạn số lần nhập sai."

### 16.4. Về forecast

"Forecast tách khoản chi tháng hiện tại thành phần chi thường xuyên và phần chi đột biến. Phần thường xuyên được nội suy theo số ngày đã quan sát, sau đó trộn với trung bình 3 tháng gần nhất theo trọng số 65/35."

### 16.5. Về cảnh báo ngân sách

"Khi tạo hoặc sửa giao dịch chi, hệ thống tính tổng chi theo danh mục trong tháng. Nếu vượt hạn mức thì tạo `BudgetAlert`, gửi realtime qua SignalR và queue email cảnh báo. Hiện tại field `AlertThresholdPercent` đã có trong model nhưng logic cảnh báo sớm theo mốc phần trăm chưa bật đầy đủ."
