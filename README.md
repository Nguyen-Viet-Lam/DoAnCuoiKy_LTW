# DoAnLTW - Finance Flow

Do an quan ly chi tieu ca nhan bang ASP.NET Core MVC, Bootstrap 5, SQL Server va Cookie Authentication.

## Tinh nang chinh

- Dang ky, dang nhap, dang xuat bang Cookie Authentication
- Xac thuc OTP qua Gmail khi dang ky va quen mat khau
- Quan ly ho so ca nhan, doi mat khau, cap nhat avatar
- Quan ly vi tien theo nhom: tien mat, ngan hang, tiet kiem
- Nhap giao dich thu chi, sua xoa va loc theo ngay, vi, danh muc, tu khoa
- Dat ngan sach theo thang va nhan canh bao khi sap vuot muc
- Dashboard tong quan, bieu do 7 ngay, giao dich gan day, so du hien co
- Bao cao theo tuan, theo thang, theo khoang ngay, xuat Excel va gui email
- Khu admin rieng cho quan ly nguoi dung, danh muc mac dinh va nhat ky he thong

## Goi y thong minh

- He thong tu dong goi y danh muc dua tren ghi chu giao dich
- Neu nguoi dung sua lai danh muc, he thong se hoc tu khoa da sua cho nhung lan sau
- Phan du bao chi tieu cuoi thang duoc tinh theo muc chi hien tai ket hop lich su gan day

## Cong nghe, thu vien, thuat toan va file can doc

Muc nay dung de on thuyet trinh, van dap va tim code nhanh.

### 1. Nen tang va thu vien

- ASP.NET Core MVC (.NET 8)
  - Khoi tao app: `DoAnLTW.Web/Program.cs:22-79`
  - Package: `DoAnLTW.Web/DoAnLTW.Web.csproj:3-18`
- Entity Framework Core + SQL Server
  - Cau hinh SQL Server: `DoAnLTW.Web/Program.cs:32-33`
  - Connection string: `DoAnLTW.Web/appsettings.Development.json:2-14`
  - DbContext: `DoAnLTW.Web/Data/FinanceDbContext.cs:7-152`
- Cookie Authentication
  - Cau hinh cookie: `DoAnLTW.Web/Program.cs:35-43`
  - Tao claim cookie khi dang nhap: `DoAnLTW.Web/Services/Auth/CookieAuthService.cs:9-30`
- SignalR
  - Dang ky SignalR: `DoAnLTW.Web/Program.cs:23`
  - Map hub: `DoAnLTW.Web/Program.cs:78`
  - Hub server: `DoAnLTW.Web/Hubs/BudgetHub.cs:7-20`
  - JS client nhan canh bao: `DoAnLTW.Web/wwwroot/js/site.js:68-80`
- ClosedXML de xuat Excel
  - Package: `DoAnLTW.Web/DoAnLTW.Web.csproj:12`
  - Tao file Excel: `DoAnLTW.Web/Services/Finance/ReportService.cs:69-97`
  - Action tai file: `DoAnLTW.Web/Controllers/ReportsController.cs:62-91`
- SMTP Gmail + email queue
  - Gui SMTP: `DoAnLTW.Web/Services/Email/SmtpEmailSender.cs:25-68`
  - Hang doi email: `DoAnLTW.Web/Services/Email/EmailQueue.cs:5-15`
  - Worker gui email nen: `DoAnLTW.Web/Services/Email/QueuedEmailBackgroundService.cs:3-37`
- Bootstrap 5, Bootstrap Icons, jQuery
  - Load CSS/JS: `DoAnLTW.Web/Views/Shared/_Layout.cshtml:21-23`, `DoAnLTW.Web/Views/Shared/_Layout.cshtml:248-251`

### 2. Co so du lieu va mo hinh du lieu

- DbSet cac bang chinh
  - `Role`, `AppUser`, `EmailOtp`, `Wallet`, `Category`, `WalletTransaction`, `Budget`, `UserPersonalKeyword`, `BudgetAlert`, `SystemLog`, `ReportDispatchLog`
  - Xem: `DoAnLTW.Web/Data/FinanceDbContext.cs:12-22`
- Index de tang toc do va tranh trung lap
  - Role name unique: `DoAnLTW.Web/Data/FinanceDbContext.cs:29-30`
  - Username unique: `DoAnLTW.Web/Data/FinanceDbContext.cs:33-34`
  - Email unique: `DoAnLTW.Web/Data/FinanceDbContext.cs:37-38`
  - Budget unique theo `UserId + CategoryId + Year + Month`: `DoAnLTW.Web/Data/FinanceDbContext.cs:44-45`
  - Tu khoa hoc rieng cua user unique theo `UserId + Keyword`: `DoAnLTW.Web/Data/FinanceDbContext.cs:48-49`
- Quan he va xoa lien ket
  - `Restrict`, `Cascade`, `SetNull`, `NoAction`
  - Xem toan bo: `DoAnLTW.Web/Data/FinanceDbContext.cs:58-152`

### 3. Xac thuc va bao mat

- Ma hoa mat khau
  - Dung `PBKDF2 + SHA256 + salt`
  - Hash: `DoAnLTW.Web/Services/Security/PasswordService.cs:10-16`
  - Verify: `DoAnLTW.Web/Services/Security/PasswordService.cs:18-35`
  - Diem de noi voi thay:
    - Khong luu plain text password
    - Moi mat khau co salt rieng
    - So sanh bang `FixedTimeEquals`
- OTP qua email
  - Sinh OTP 6 so bang `RandomNumberGenerator`: `DoAnLTW.Web/Services/Auth/OtpService.cs:23`
  - Khong luu OTP goc, chi luu hash SHA256: `DoAnLTW.Web/Services/Auth/OtpService.cs:29-34`, `DoAnLTW.Web/Services/Auth/OtpService.cs:98-101`
  - Het han sau 10 phut: `DoAnLTW.Web/Services/Auth/OtpService.cs:31`
  - Gioi han so lan nhap sai: `DoAnLTW.Web/Services/Auth/OtpService.cs:72-77`
- Cookie Authentication + claims
  - Tao claim `NameIdentifier`, `Name`, `GivenName`, `Email`, `Role`: `DoAnLTW.Web/Services/Auth/CookieAuthService.cs:13-19`
  - SignIn luu cookie: `DoAnLTW.Web/Services/Auth/CookieAuthService.cs:24-30`

### 4. Luong dang ky, dang nhap, quen mat khau

- Dang ky
  - Validate trung email/username: `DoAnLTW.Web/Controllers/AuthController.cs:102-111`
  - Hash password va tao user: `DoAnLTW.Web/Controllers/AuthController.cs:118-130`
  - Gui OTP: `DoAnLTW.Web/Controllers/AuthController.cs:133`
  - Ghi log: `DoAnLTW.Web/Controllers/AuthController.cs:134`
- Xac thuc OTP khi dang ky
  - Action: `DoAnLTW.Web/Controllers/AuthController.cs:141-177`
- Dang nhap
  - Kiem tra password: `DoAnLTW.Web/Controllers/AuthController.cs:61`
  - Kiem tra khoa tai khoan: `DoAnLTW.Web/Controllers/AuthController.cs:67-71`
  - Kiem tra da xac thuc email chua: `DoAnLTW.Web/Controllers/AuthController.cs:73-78`
  - Tao cookie dang nhap: `DoAnLTW.Web/Controllers/AuthController.cs:80`
  - Dieu huong theo role: `DoAnLTW.Web/Controllers/AuthController.cs:273-283`
- Quen mat khau / dat lai mat khau
  - Gui OTP doi mat khau: `DoAnLTW.Web/Controllers/AuthController.cs:203-217`
  - Verify OTP roi hash mat khau moi: `DoAnLTW.Web/Controllers/AuthController.cs:229-253`

### 5. Phan quyen

- Admin
  - `DoAnLTW.Web/Controllers/AdminController.cs:11`
- User
  - `DoAnLTW.Web/Controllers/WalletsController.cs:11`
  - `DoAnLTW.Web/Controllers/TransactionsController.cs:11`
  - `DoAnLTW.Web/Controllers/BudgetsController.cs:11`
  - `DoAnLTW.Web/Controllers/ReportsController.cs:12`
- User, Admin dung chung
  - `DoAnLTW.Web/Controllers/DashboardController.cs:10`
  - `DoAnLTW.Web/Controllers/ProfileController.cs:11`

### 6. Goi y thong minh va "AI nhe"

- Ban chat
  - Khong dung mo hinh AI nang
  - Dung `rule-based + hoc tu hanh vi nguoi dung`
- Rule theo tu khoa
  - Rule chi tieu: `DoAnLTW.Web/Services/Finance/CategorizationService.cs:10-32`
  - Rule thu nhap: `DoAnLTW.Web/Services/Finance/CategorizationService.cs:34-42`
- Cac buoc goi y
  - Chuan hoa chuoi, bo dau, tach tu khoa: `DoAnLTW.Web/Services/Finance/CategorizationService.cs:49-95`, `DoAnLTW.Web/Services/Finance/CategorizationService.cs:135-166`
  - Uu tien tu khoa ca nhan da hoc: `DoAnLTW.Web/Services/Finance/CategorizationService.cs:58-72`
  - Neu khong co thi moi dung rule mac dinh: `DoAnLTW.Web/Services/Finance/CategorizationService.cs:86-95`
- Hoc tu nguoi dung
  - Luu lai tu khoa va danh muc da sua: `DoAnLTW.Web/Services/Finance/CategorizationService.cs:97-133`
  - Bang DB: `DoAnLTW.Web/Data/FinanceDbContext.cs:19`, `DoAnLTW.Web/Data/FinanceDbContext.cs:48-49`
- API goi y cho form giao dich
  - `DoAnLTW.Web/Controllers/TransactionsController.cs:193-210`

### 7. Thuat toan / logic tinh toan trong tai chinh

- Cap nhat so du vi
  - Cong / tru tien khi them giao dich: `DoAnLTW.Web/Controllers/TransactionsController.cs:152-157`
  - Dao nguoc so du khi sua / xoa: `DoAnLTW.Web/Controllers/TransactionsController.cs:99-111`, `DoAnLTW.Web/Controllers/TransactionsController.cs:177`
  - Ham tinh: `DoAnLTW.Web/Controllers/TransactionsController.cs:353-371`
- Tong hop dashboard
  - Group chi tieu theo danh muc: `DoAnLTW.Web/Controllers/DashboardController.cs:51-61`
  - Tao bieu do 7 ngay bang `Enumerable.Range`: `DoAnLTW.Web/Controllers/DashboardController.cs:65-75`
  - Tao so sanh 12 thang: `DoAnLTW.Web/Controllers/DashboardController.cs:81-95`
- Du bao chi tieu cuoi thang
  - Tinh theo `run-rate hien tai` ket hop `trung binh 3 thang truoc`
  - Xem: `DoAnLTW.Web/Services/Finance/ForecastService.cs:14-41`
- Tinh ngan sach va canh bao
  - Tinh `% da dung = spent / limit * 100`: `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:47-49`
  - Neu vuot nguong thi tao alert: `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:50-85`

### 8. Realtime bang SignalR

- Them user vao group theo user id: `DoAnLTW.Web/Hubs/BudgetHub.cs:11-18`
- Gui canh bao tu server: `DoAnLTW.Web/Services/Finance/BudgetMonitorService.cs:87-95`
- Client nhan su kien `budgetAlert` va hien toast: `DoAnLTW.Web/wwwroot/js/site.js:68-80`

### 9. Bao cao va xuat Excel

- Tong hop bao cao theo khoang ngay
  - `DoAnLTW.Web/Services/Finance/ReportService.cs:15-58`
- Group top category trong bao cao
  - `DoAnLTW.Web/Services/Finance/ReportService.cs:43-52`
- Xuat Excel bang ClosedXML
  - `DoAnLTW.Web/Services/Finance/ReportService.cs:69-97`
- Action export file
  - `DoAnLTW.Web/Controllers/ReportsController.cs:62-91`
- Gui bao cao qua email
  - Gui bao cao tuan: `DoAnLTW.Web/Controllers/ReportsController.cs:96-117`
  - Gui bao cao khoang ngay: `DoAnLTW.Web/Controllers/ReportsController.cs:120-151`

### 10. Email async va background service

- Hang doi email dung `Channel`
  - `DoAnLTW.Web/Services/Email/EmailQueue.cs:5-15`
- Worker nen de gui mail
  - `DoAnLTW.Web/Services/Email/QueuedEmailBackgroundService.cs:17-37`
- SMTP sender
  - Tao `SmtpClient`: `DoAnLTW.Web/Services/Email/SmtpEmailSender.cs:44-63`
  - Neu dev ma SMTP loi thi luu file HTML vao `App_Data/DevEmails`: `DoAnLTW.Web/Services/Email/SmtpEmailSender.cs:72-97`
- Gui bao cao tu dong thu 2 hang tuan
  - `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs:18-29`
  - Dieu kien gui thu 2, 8h-9h: `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs:34-39`
  - Chong gui trung bang `ReportDispatchLog`: `DoAnLTW.Web/Services/Finance/WeeklyReportHostedService.cs:61-66`

### 11. Admin va nhat ky he thong

- Dashboard admin
  - `DoAnLTW.Web/Controllers/AdminController.cs:22-80`
- Loc nguoi dung theo tu khoa, trang thai, role
  - `DoAnLTW.Web/Controllers/AdminController.cs:83-144`
- Quan ly danh muc
  - Danh sach va form: `DoAnLTW.Web/Controllers/AdminController.cs:148-200`
  - Luu category: `DoAnLTW.Web/Controllers/AdminController.cs:269-347`
  - Xoa category: `DoAnLTW.Web/Controllers/AdminController.cs:349-378`
- Xem log he thong
  - `DoAnLTW.Web/Controllers/AdminController.cs:202-234`
  - Query log: `DoAnLTW.Web/Controllers/AdminController.cs:380-395`
- Ghi log he thong
  - `DoAnLTW.Web/Services/Finance/AuditLogService.cs:15-33`

### 12. Seed du lieu va tai khoan demo

- Khoi tao seed khi app chay
  - `DoAnLTW.Web/Program.cs:63`
  - `DoAnLTW.Web/Data/SeedData.cs:7-14`
- Tu tao DB bang `EnsureCreatedAsync`
  - `DoAnLTW.Web/Data/SeedData.cs:11`
- Tao role `Admin`, `User`
  - `DoAnLTW.Web/Data/Seeders/ReferenceDataSeeder.cs:18-22`
- Tao admin mac dinh tu `AdminAccount`
  - `DoAnLTW.Web/Data/Seeders/ReferenceDataSeeder.cs:28-57`
- Tao category mac dinh
  - `DoAnLTW.Web/Data/Seeders/ReferenceDataSeeder.cs:63-80`
- Tao user demo, vi demo, giao dich demo, budget demo
  - `DoAnLTW.Web/Data/Seeders/DemoFinanceDataSeeder.cs:22-23`
  - `DoAnLTW.Web/Data/Seeders/DemoFinanceDataSeeder.cs:62-100`
  - `DoAnLTW.Web/Data/Seeders/DemoFinanceDataSeeder.cs:103-140`
  - `DoAnLTW.Web/Data/Seeders/DemoFinanceDataSeeder.cs:146-218`

### 13. Upload avatar

- Luu file vao `wwwroot/uploads/avatars`
  - `DoAnLTW.Web/Services/Finance/AvatarStorageService.cs:26`
- Chi cho phep `.jpg`, `.jpeg`, `.png`, `.webp`
  - `DoAnLTW.Web/Services/Finance/AvatarStorageService.cs:19-24`
- Ghi file moi va xoa file cu
  - `DoAnLTW.Web/Services/Finance/AvatarStorageService.cs:29-42`

### 14. Goi y cach trinh bay voi thay

- Neu thay hoi "AI la gi?" thi tra loi:
  - Day la he thong goi y thong minh dua tren tu khoa va lich su sua cua nguoi dung, khong phai mo hinh hoc may nang.
- Neu thay hoi "bao mat mat khau?" thi tra loi:
  - Em dung PBKDF2, co salt rieng va khong luu plain text password.
- Neu thay hoi "OTP luu the nao?" thi tra loi:
  - OTP sinh ngau nhien, luu hash SHA256, co thoi han va gioi han so lan nhap sai.
- Neu thay hoi "realtime la gi?" thi tra loi:
  - Khi giao dich lam vuot muc ngan sach, server day canh bao ngay qua SignalR, trinh duyet nhan va hien toast ma khong can reload.

## Cau truc thu muc

- `DoAnLTW.Web/Controllers`: xu ly tung module
- `DoAnLTW.Web/Data`: `FinanceDbContext`, seed du lieu, cau hinh DB
- `DoAnLTW.Web/Models/Entities`: entity va bang du lieu
- `DoAnLTW.Web/Models/ViewModels`: model cho tung man hinh
- `DoAnLTW.Web/Services`: OTP, email, bao cao, goi y danh muc, du bao chi tieu
- `DoAnLTW.Web/Hubs`: SignalR cho canh bao ngan sach
- `DoAnLTW.Web/Views`: Razor view
- `DoAnLTW.Web/wwwroot`: CSS, JS, avatar upload

## Tai khoan demo

- Admin
  - Email: `admin@financeflow.local`
  - Password: `Admin123!`
- User demo
  - Email: `demo@financeflow.local`
  - Password: `User123!`
- User demo phu
  - Email: `trang@financeflow.local`
  - Password: `User123!`

## Cau hinh SQL Server

- Server: `localhost,1433`
- Database: `DoAnLTWFinanceDb`
- Dang nhap SSMS
  - `Authentication`: `SQL Server Authentication`
  - `Login`: `sa`
  - `Password`: `Thpasssql123`
  - `Encrypt=False;TrustServerCertificate=True;`

## Cach chay local

### Cach 1 - Visual Studio

1. Mo `DoAnLTW.sln`
2. Chon project khoi dong `DoAnLTW.Web`
3. Nhan `F5` hoac `Ctrl + F5`
4. Mo `http://localhost:5055`

### Cach 2 - Command line

1. Mo terminal tai thu muc repo
2. Chay `dotnet build DoAnLTW.Web/DoAnLTW.Web.csproj`
3. Chay `dotnet run --project DoAnLTW.Web/DoAnLTW.Web.csproj --launch-profile http`
4. Mo `http://localhost:5055`

Neu thay loi file bi khoa khi build, hay dung ban web cu truoc:

- PowerShell: `Stop-Process -Name DoAnLTW.Web -Force`
- Hoac dong cua so terminal/Visual Studio dang chay app

## Ghi chu dev

- App tu tao du lieu mau neu database chua co
- Neu Gmail bi chan khi chay local, email dev se duoc luu tai `DoAnLTW.Web/App_Data/DevEmails`
- Cac file log va thu muc chay thu da duoc dua vao `.gitignore`, khong nen commit len repo
