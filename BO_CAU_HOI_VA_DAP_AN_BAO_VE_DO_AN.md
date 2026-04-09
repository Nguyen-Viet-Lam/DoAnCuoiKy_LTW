# Bộ Câu Hỏi Và Đáp Án Bảo Vệ Đồ Án DoAnLTW

Tài liệu này được viết theo kiểu giáo viên hỏi, sinh viên trả lời.
Mục tiêu là giúp bạn:

- ôn kiến thức môn Lập trình Web,
- hiểu rõ kiến trúc dự án,
- trả lời được câu hỏi phản biện về bảo mật, OTP, SMTP Gmail, SignalR, báo cáo, dự báo chi tiêu,
- nắm được ý nghĩa các con số, tham số, ngưỡng và biết nếu thay đổi thì hệ thống sẽ bị ảnh hưởng thế nào.

Phạm vi tài liệu bám theo mã nguồn hiện tại của dự án `DoAnLTW.Web`.

## 1. Câu hỏi tổng quan kiến trúc

### Câu 1. Dự án này được xây dựng theo mô hình gì?

**Trả lời mẫu:**

Dự án được xây dựng theo mô hình ASP.NET Core MVC trên .NET 8. Trong đó:

- `Model` gồm Entity, ViewModel, DbContext và dữ liệu nghiệp vụ.
- `View` là Razor View để hiển thị giao diện.
- `Controller` nhận request, xử lý logic điều phối và trả về view hoặc file/json.

Ngoài MVC, hệ thống còn kết hợp:

- Entity Framework Core để làm việc với SQL Server,
- Cookie Authentication để xác thực,
- SignalR để gửi cảnh báo realtime,
- BackgroundService để xử lý tác vụ nền,
- ClosedXML để xuất Excel,
- SMTP để gửi email OTP và báo cáo.

### Câu 2. Điểm khởi động của dự án nằm ở đâu?

**Trả lời mẫu:**

Điểm khởi động nằm ở `DoAnLTW.Web/Program.cs`.

File này làm các việc chính:

- đọc cấu hình,
- đăng ký DI,
- cấu hình xác thực cookie,
- đăng ký EF Core DbContext,
- bật SignalR,
- bật background service,
- chạy migrate và seed dữ liệu,
- cấu hình middleware,
- map route controller và map hub SignalR.

### Câu 3. Những nhóm chức năng lớn của hệ thống là gì?

**Trả lời mẫu:**

Hệ thống có 4 nhóm lớn:

1. Nhóm thiết lập tài khoản:
   - đăng ký,
   - xác thực OTP,
   - đăng nhập,
   - quên mật khẩu,
   - cập nhật hồ sơ cá nhân.
2. Nhóm quản lý tài chính:
   - quản lý ví,
   - quản lý giao dịch,
   - quản lý ngân sách.
3. Nhóm phân tích và cảnh báo:
   - dashboard,
   - báo cáo,
   - xuất Excel,
   - gửi email báo cáo,
   - cảnh báo vượt ngân sách,
   - cảnh báo ví xuống thấp,
   - dự báo chi tiêu cuối tháng.
4. Nhóm quản trị:
   - dashboard admin,
   - quản lý user,
   - quản lý category,
   - xem log hệ thống.

### Câu 4. Tại sao dự án lại tách thành `Controllers`, `Services`, `Data`, `Models`?

**Trả lời mẫu:**

Mục tiêu là tách trách nhiệm:

- `Controllers` lo điều phối request và response.
- `Services` chứa nghiệp vụ chính.
- `Data` chứa DbContext, seeding, migration.
- `Models` chứa dữ liệu dùng cho DB và UI.

Cách tách này giúp:

- code dễ đọc hơn,
- dễ bảo trì,
- ít lặp logic,
- thuận tiện mở rộng tính năng.

## 2. Câu hỏi về môn Lập trình Web

### Câu 5. Trong dự án này, request của người dùng đi qua những bước nào?

**Trả lời mẫu:**

Một request điển hình sẽ đi qua các bước:

1. Trình duyệt gửi HTTP request đến server.
2. ASP.NET Core middleware xử lý route, authentication, authorization.
3. Controller action phù hợp được gọi.
4. Model binding ánh xạ dữ liệu form/query string vào ViewModel.
5. Controller kiểm tra `ModelState`.
6. Controller gọi service hoặc DbContext để xử lý nghiệp vụ.
7. Kết quả được trả về dưới dạng View, Redirect, Json hoặc File.

### Câu 6. `ModelState` dùng để làm gì?

**Trả lời mẫu:**

`ModelState` dùng để kiểm tra dữ liệu đầu vào có hợp lệ hay không theo các rule trong `DataAnnotations`.

Ví dụ:

- `[Required]`,
- `[Range]`,
- `[EmailAddress]`,
- `[StringLength]`,
- `[Compare]`.

Nếu `ModelState` không hợp lệ thì controller thường trả lại view để người dùng sửa dữ liệu.

### Câu 7. Tại sao dự án dùng ViewModel mà không truyền thẳng Entity ra View?

**Trả lời mẫu:**

Vì ViewModel giúp:

- chỉ đưa ra đúng dữ liệu giao diện cần,
- tránh lộ toàn bộ cấu trúc database,
- dễ validate dữ liệu nhập,
- gom nhiều nguồn dữ liệu khác nhau vào một model hiển thị.

Entity chủ yếu phục vụ cho database, còn ViewModel phục vụ cho UI.

### Câu 8. Sự khác nhau giữa `Entity` và `ViewModel` trong dự án này là gì?

**Trả lời mẫu:**

- `Entity` là lớp ánh xạ với bảng trong database.
- `ViewModel` là lớp phục vụ hiển thị hoặc nhập liệu cho giao diện.

Ví dụ:

- `WalletTransaction` là entity lưu vào DB.
- `TransactionFormViewModel` là model nhận dữ liệu từ form nhập giao dịch.
- `TransactionHistoryItemViewModel` là model hiển thị danh sách giao dịch.

### Câu 9. Dự án dùng cơ chế xác thực nào?

**Trả lời mẫu:**

Dự án dùng `Cookie Authentication`.

Sau khi đăng nhập thành công:

- hệ thống tạo claims cho user,
- ghi cookie xác thực vào trình duyệt,
- các request sau sẽ dùng cookie đó để nhận diện người dùng.

### Câu 10. Dự án dùng cơ chế phân quyền nào?

**Trả lời mẫu:**

Dự án dùng `Authorization` theo role với attribute `[Authorize]`.

Ví dụ:

- `[Authorize(Roles = "User")]` cho các controller nghiệp vụ người dùng,
- `[Authorize(Roles = "Admin")]` cho khu vực quản trị,
- `[Authorize(Roles = "User,Admin")]` cho các trang dùng chung như profile hoặc dashboard tổng quát.

### Câu 11. Vì sao phải có `LoginPath` và `AccessDeniedPath`?

**Trả lời mẫu:**

- `LoginPath` dùng khi người dùng chưa đăng nhập mà cố truy cập tài nguyên cần xác thực.
- `AccessDeniedPath` dùng khi người dùng đã đăng nhập nhưng không đủ quyền.

Điều này giúp hệ thống điều hướng rõ ràng và thân thiện hơn.

## 3. Câu hỏi về bảo mật, Cookie Authentication, mật khẩu và OTP

### Câu 12. Cookie Authentication được cấu hình ở đâu?

**Trả lời mẫu:**

Cookie Authentication được cấu hình trong `Program.cs`:

- `AddAuthentication(...)`
- `AddCookie(...)`

Các thông số chính:

- `LoginPath = "/Auth/Login"`
- `AccessDeniedPath = "/Auth/AccessDenied"`
- `SlidingExpiration = true`
- `ExpireTimeSpan = 14 ngày`

Luồng cookie của dự án thực tế nằm ở 4 chỗ chính:

- `Program.cs`: cấu hình scheme cookie và middleware.
- `CookieAuthService.cs`: tạo claims, ghi cookie, xóa cookie.
- `AuthController.cs`: gọi đăng nhập và đăng xuất.
- `ClaimsPrincipalExtensions.cs`: đọc lại `UserId`, `DisplayName` từ claims.

Ngoài phần cấu hình, `Program.cs` còn có:

- `app.UseAuthentication()`
- `app.UseAuthorization()`

Ý nghĩa là ở mỗi request sau này, middleware sẽ đọc cookie từ trình duyệt, giải mã authentication ticket rồi dựng lại `HttpContext.User` để controller và `[Authorize]` sử dụng.

### Câu 13. `SlidingExpiration = true` có ý nghĩa gì?

**Trả lời mẫu:**

Nó nghĩa là nếu người dùng tiếp tục hoạt động thì thời hạn cookie có thể được gia hạn thêm.

Lợi ích:

- người dùng không bị đăng xuất quá nhanh khi vẫn đang sử dụng hệ thống.

Nếu đổi thành `false`:

- cookie hết hạn cứng theo thời gian,
- user dễ bị đăng xuất giữa lúc đang dùng.

Nói dễ hiểu:

- nếu user vẫn mở hệ thống và liên tục gửi request hợp lệ, hệ thống có thể kéo dài tuổi thọ phiên đăng nhập,
- nhờ đó trải nghiệm mượt hơn, nhất là với web quản lý dùng lâu trong ngày,
- nhưng nếu máy dùng chung thì cũng cần cân nhắc vì phiên có thể tồn tại lâu hơn.

### Câu 14. Tại sao thời gian cookie lại để là 14 ngày?

**Trả lời mẫu:**

14 ngày là mức cân bằng giữa:

- tiện lợi cho người dùng,
- và mức độ an toàn chấp nhận được.

Nếu tăng lên quá cao:

- cookie tồn tại lâu hơn,
- rủi ro tăng nếu máy bị người khác dùng chung.

Nếu giảm quá thấp:

- người dùng sẽ phải đăng nhập lại thường xuyên.

Tuy nhiên trong source hiện tại cần hiểu thêm 1 điểm:

- `Program.cs` đặt mặc định `ExpireTimeSpan = 14 ngày`,
- nhưng `CookieAuthService.SignInAsync(...)` lại set thêm `ExpiresUtc = DateTimeOffset.UtcNow.AddDays(rememberMe ? 14 : 1)`.

Vì vậy thời hạn thực tế đang là:

- nếu `RememberMe = true`: khoảng 14 ngày,
- nếu `RememberMe = false`: khoảng 1 ngày.

Nghĩa là:

- `ExpireTimeSpan` là cấu hình chung của scheme,
- `ExpiresUtc` là hạn cụ thể của lần đăng nhập hiện tại,
- trong dự án này, `ExpiresUtc` đang quyết định rõ phiên có nhớ đăng nhập hay không.

### Câu 15. Mật khẩu trong hệ thống có lưu plain text không?

**Trả lời mẫu:**

Không. Mật khẩu được băm bằng PBKDF2 trước khi lưu.

Hệ thống không lưu mật khẩu gốc.

### Câu 16. Vì sao dự án dùng PBKDF2 mà không chỉ dùng SHA-256 đơn thuần cho mật khẩu?

**Trả lời mẫu:**

PBKDF2 phù hợp cho password hơn vì:

- có salt riêng,
- có nhiều vòng lặp,
- làm chậm brute force hơn.

Nếu chỉ dùng SHA-256 một lần:

- tốc độ tính hash quá nhanh,
- kẻ tấn công có thể dò password nhanh hơn nhiều nếu lộ database.

Cách hoạt động trong dự án là:

1. `PasswordService.Hash(...)` sinh ra một `salt` ngẫu nhiên riêng cho từng user.
2. Hệ thống chạy `Rfc2898DeriveBytes.Pbkdf2(...)` với `SHA256` ở bên trong.
3. Thuật toán lặp lại `100_000` vòng để cố tình làm chậm việc thử password hàng loạt.
4. Kết quả được lưu thành chuỗi dạng:
   - `v1.iterations.salt.hash`
5. Khi login, `PasswordService.Verify(...)` tách chuỗi này ra, lấy lại `iterations` và `salt`, rồi băm lại password người dùng vừa nhập để so sánh.

Việc so sánh còn dùng `CryptographicOperations.FixedTimeEquals(...)` để giảm rủi ro timing attack.

### Câu 17. Các thông số trong `PasswordService` có ý nghĩa gì?

**Trả lời mẫu:**

Các thông số chính:

- `SaltSize = 16`: độ dài salt 16 byte.
- `HashSize = 32`: độ dài hash đầu ra 32 byte.
- `Iterations = 100_000`: số vòng lặp PBKDF2.

Ý nghĩa:

- salt giúp cùng một password cho ra hash khác nhau giữa các user,
- iterations làm quá trình băm chậm hơn để chống brute force,
- hash size quyết định độ dài kết quả lưu trữ.

Ngoài ra format lưu trữ `v1.iterations.salt.hash` còn có ý nghĩa:

- `v1`: version của format hash, để sau này có thể nâng cấp thuật toán mà vẫn nhận diện được dữ liệu cũ,
- `iterations`: lưu cùng với hash để lúc verify biết phải băm lại bao nhiêu vòng,
- `salt` và `hash` được chuyển sang Base64 để lưu trong DB dưới dạng chuỗi.

### Câu 18. Nếu giảm `Iterations` từ `100_000` xuống thấp thì sao?

**Trả lời mẫu:**

Nếu giảm xuống, ví dụ còn `10_000`:

- đăng nhập có thể nhanh hơn một chút,
- nhưng độ an toàn giảm vì việc dò hash cũng nhanh hơn nhiều.

Nếu tăng lên quá cao:

- độ an toàn tăng,
- nhưng đăng nhập và đổi mật khẩu cũng sẽ chậm hơn.

### Câu 19. OTP được tạo như thế nào?

**Trả lời mẫu:**

OTP được sinh ngẫu nhiên bằng:

- `RandomNumberGenerator.GetInt32(100000, 999999)`

Nghĩa là mã có 6 chữ số.

### Câu 20. Tại sao OTP chỉ có 6 chữ số?

**Trả lời mẫu:**

6 chữ số là mức phổ biến vì:

- đủ ngắn để người dùng nhập nhanh,
- vẫn có không gian mã đủ lớn cho xác thực ngắn hạn.

Nếu tăng lên 8 chữ số:

- an toàn hơn một chút,
- nhưng người dùng nhập khó hơn.

Nếu giảm xuống 4 chữ số:

- nhập nhanh hơn,
- nhưng khả năng đoán trúng cao hơn nhiều.

### Câu 21. OTP có lưu trực tiếp trong DB không?

**Trả lời mẫu:**

Không. OTP được băm bằng SHA-256 rồi mới lưu vào `EmailOtp.CodeHash`.

Điều này giúp:

- nếu DB bị lộ, mã OTP gốc không lộ trực tiếp.

### Câu 22. Vì sao OTP hết hạn sau 10 phút?

**Trả lời mẫu:**

10 phút là mức hợp lý vì:

- đủ thời gian để người dùng mở email và nhập mã,
- nhưng không quá dài để mã tồn tại lâu.

Nếu tăng lên 30 phút:

- người dùng thoải mái hơn,
- nhưng rủi ro OTP bị lạm dụng cũng tăng.

Nếu giảm xuống 2 phút:

- an toàn hơn,
- nhưng người dùng dễ bị hết hạn trước khi kịp nhập.

### Câu 23. Tại sao hệ thống lại giới hạn số lần nhập OTP?

**Trả lời mẫu:**

Hệ thống tăng `Attempts` mỗi lần kiểm tra và khóa nếu quá 5 lần.

Mục tiêu:

- chống đoán mò OTP,
- giảm brute force.

Nếu bỏ giới hạn:

- kẻ tấn công có thể thử liên tục.

### Câu 24. SMTP Gmail trong dự án hoạt động thế nào?

**Trả lời mẫu:**

Dự án không dùng Gmail API mà dùng SMTP.

Cụ thể:

- cấu hình trong `SmtpOptions`,
- host mặc định là `smtp.gmail.com`,
- port là `587`,
- `EnableSsl = true`.

Nghĩa là hệ thống gửi mail như một mail client thông thường thông qua server SMTP của Gmail.

Luồng gửi mail thực tế là:

1. Các service như `OtpService`, `BudgetMonitorService`, `WeeklyReportHostedService` tạo `EmailMessage`.
2. Email không được gửi ngay trong request mà được đưa vào `EmailQueue`.
3. `QueuedEmailBackgroundService` chạy nền, lấy mail ra từng cái một.
4. Service này resolve `IEmailSender`.
5. `SmtpEmailSender` mới là nơi mở kết nối tới `smtp.gmail.com` và gọi `SendMailAsync(...)`.

Thiết kế này giúp:

- request đăng ký, tạo giao dịch, gửi báo cáo phản hồi nhanh hơn,
- nếu SMTP chậm thì không làm treo luồng chính của người dùng,
- cùng một cơ chế gửi mail có thể tái sử dụng cho OTP, cảnh báo ngân sách và báo cáo tuần.

### Câu 25. Nếu đổi `Port = 587` sang port khác thì sao?

**Trả lời mẫu:**

Port `587` thường dùng cho SMTP submission với TLS.

Nếu đổi sai port:

- kết nối SMTP có thể thất bại,
- OTP và email báo cáo sẽ không gửi được.

### Câu 26. Vì sao hệ thống dùng `EmailQueue` thay vì gửi mail trực tiếp trong controller?

**Trả lời mẫu:**

Vì gửi mail là tác vụ I/O chậm.

Nếu gửi trực tiếp trong request:

- người dùng phải chờ SMTP phản hồi,
- request dễ chậm hoặc timeout.

Khi dùng queue:

- controller chỉ đưa mail vào hàng đợi,
- background service gửi sau,
- trải nghiệm người dùng nhanh hơn.

Trong code, các thành phần chính là:

- `EmailQueue.cs`: hàng đợi in-memory.
- `QueuedEmailBackgroundService.cs`: worker chạy nền.
- `SmtpEmailSender.cs`: gửi mail thực tế qua SMTP.

Ví dụ:

- `OtpService` dùng queue để gửi OTP,
- `BudgetMonitorService` dùng queue để gửi email vượt ngân sách,
- `WeeklyReportHostedService` dùng queue để gửi báo cáo tuần.

## 4. Câu hỏi về SignalR và cảnh báo thời gian thực

### Câu 27. SignalR là gì trong dự án này?

**Trả lời mẫu:**

SignalR là cơ chế realtime giúp server đẩy dữ liệu xuống trình duyệt ngay khi có sự kiện mới.

Trong đồ án này, SignalR dùng để:

- cảnh báo vượt ngân sách,
- cảnh báo ví xuống dưới ngưỡng thấp.

Các file chính liên quan là:

- `Program.cs`: bật `AddSignalR()` và map hub.
- `BudgetHub.cs`: hub realtime của dự án.
- `site.js`: client JavaScript kết nối và nhận event.
- `BudgetMonitorService.cs`: phát `budgetAlert`.
- `WalletBalanceMonitorService.cs`: phát `walletAlert`.

### Câu 28. `app.MapHub<BudgetHub>("/budgetHub")` có nghĩa là gì?

**Trả lời mẫu:**

Nó có nghĩa là:

- map một endpoint realtime tại đường dẫn `/budgetHub`,
- mọi client muốn kết nối tới `BudgetHub` sẽ đi qua URL này.

Nếu không có dòng này:

- frontend sẽ không kết nối được tới hub,
- cảnh báo realtime sẽ không hoạt động.

Cần phân biệt:

- `MapControllerRoute(...)` là route cho controller/action HTTP thông thường,
- `MapHub<BudgetHub>(...)` là endpoint dành riêng cho kết nối realtime.

Phía client trong `site.js` đang gọi:

- `.withUrl("/budgetHub")`

Nên nếu đổi đường dẫn ở server mà không sửa ở client thì SignalR sẽ mất kết nối.

### Câu 29. `BudgetHub` trong dự án có vai trò gì?

**Trả lời mẫu:**

`BudgetHub` là điểm kết nối realtime giữa server và trình duyệt.

Nó không chứa nghiệp vụ tính toán chính.

Vai trò chính của nó là:

- xác định user đang kết nối,
- đưa connection vào group riêng theo user.

Điểm quan trọng là `BudgetHub` không chứa nghiệp vụ tính ngân sách.

Nghiệp vụ chính vẫn nằm ở service:

- `BudgetMonitorService`
- `WalletBalanceMonitorService`

`BudgetHub` chỉ đóng vai trò như “đường dây” realtime để server gửi đúng thông báo cho đúng người dùng.

### Câu 30. Tại sao phải chia group theo `user-{userId}`?

**Trả lời mẫu:**

Vì hệ thống chỉ muốn gửi cảnh báo cho đúng người liên quan.

Nếu không chia group:

- rất dễ broadcast nhầm cảnh báo cho toàn bộ client.

### Câu 31. Event `budgetAlert` được phát khi nào?

**Trả lời mẫu:**

Nó được phát khi:

- người dùng tạo hoặc sửa giao dịch chi,
- hệ thống tính thấy danh mục đó đã vượt ngân sách tháng.

Service phát event là `BudgetMonitorService`.

### Câu 32. Event `walletAlert` được phát khi nào?

**Trả lời mẫu:**

Nó được phát khi:

- ví vừa cắt xuống dưới ngưỡng thấp,
- tức là trước đó số dư còn trên ngưỡng, sau thao tác thì xuống dưới ngưỡng.

Nhờ vậy hệ thống tránh spam cảnh báo lặp lại.

### Câu 33. Ngưỡng cảnh báo ví thấp là bao nhiêu?

**Trả lời mẫu:**

Hiện tại là:

- `LowBalanceThreshold = 200000m`

Nếu tăng ngưỡng này:

- hệ thống cảnh báo sớm hơn,
- nhưng có thể cảnh báo nhiều hơn.

Nếu giảm ngưỡng này:

- hệ thống bớt nhạy,
- nhưng có thể cảnh báo quá muộn.

Giải thích thêm:

- hậu tố `m` nghĩa là kiểu `decimal`, phù hợp cho tiền vì tránh sai số của `float` hoặc `double`,
- đây là một business rule đang hard-code trong `WalletBalanceMonitorService`,
- code hiện tại chỉ cảnh báo khi số dư “vừa cắt xuống dưới ngưỡng”, không cảnh báo lặp lại liên tục khi ví vẫn đang thấp.

Ví dụ:

- trước giao dịch: `350.000`,
- sau giao dịch: `150.000`,
- hệ thống sẽ bắn cảnh báo.

Nhưng nếu:

- trước đó ví đã là `150.000`,
- giao dịch mới làm còn `120.000`,
- hệ thống sẽ không bắn lại để tránh spam.

## 5. Câu hỏi về báo cáo, ClosedXML và Excel

### Câu 34. Dự án dùng thư viện gì để xuất Excel?

**Trả lời mẫu:**

Dự án dùng:

- `ClosedXML`

Thư viện này được khai báo trong file `.csproj`.

### Câu 35. Tại sao chọn ClosedXML?

**Trả lời mẫu:**

Vì ClosedXML:

- dễ thao tác hơn so với OpenXML thuần,
- phù hợp cho các file Excel báo cáo dạng bảng,
- dễ tạo sheet, cell, format ngày và số.

### Câu 36. Báo cáo Excel lấy dữ liệu từ đâu?

**Trả lời mẫu:**

Controller gọi `ReportService.ExportExcelAsync`.

Service này:

1. gọi `BuildSummaryAsync`,
2. tổng hợp dữ liệu thu chi,
3. ghi dữ liệu vào workbook Excel,
4. trả về `byte[]`.

### Câu 37. Vì sao báo cáo dùng khoảng ngày `[from, to]` nhưng query lại dùng `endExclusive`?

**Trả lời mẫu:**

Service dùng:

- `from.Date`
- `to.Date.AddDays(1)`

Rồi query theo:

- `OccurredOn >= from`
- `OccurredOn < endExclusive`

Mẫu này giúp:

- bao trọn ngày cuối cùng,
- tránh lỗi do phần giờ phút giây.

### Câu 38. Ý nghĩa của `AverageExpensePerDay` là gì?

**Trả lời mẫu:**

Đó là:

- tổng chi trong khoảng chia cho số ngày trong khoảng,
- sau đó làm tròn.

Chỉ số này giúp nhìn nhanh cường độ chi tiêu trung bình theo ngày.

## 6. Câu hỏi về background service và xử lý bất đồng bộ

### Câu 39. Dự án có những background service nào?

**Trả lời mẫu:**

Có 2 background service chính:

- `QueuedEmailBackgroundService`
- `WeeklyReportHostedService`

### Câu 40. `QueuedEmailBackgroundService` làm gì?

**Trả lời mẫu:**

Nó chạy nền và lặp liên tục:

1. chờ email trong `EmailQueue`,
2. lấy email ra,
3. resolve `IEmailSender`,
4. gửi mail.

### Câu 41. `WeeklyReportHostedService` làm gì?

**Trả lời mẫu:**

Nó định kỳ kiểm tra xem có đến thời điểm gửi báo cáo tuần hay chưa.

Nếu đúng điều kiện:

- nó lấy danh sách user,
- tạo nội dung báo cáo,
- đưa email vào queue,
- ghi lại lịch sử gửi vào `ReportDispatchLog`.

### Câu 42. Vì sao worker báo cáo tuần kiểm tra mỗi 30 phút?

**Trả lời mẫu:**

Đây là mức kiểm tra vừa phải:

- không quá dày để tốn tài nguyên,
- không quá thưa để bỏ lỡ khung giờ gửi.

Nếu giảm xuống 1 phút:

- phản ứng nhanh hơn,
- nhưng tốn tài nguyên hơn và lặp kiểm tra nhiều hơn.

Nếu tăng lên vài giờ:

- nhẹ hơn,
- nhưng dễ bỏ lỡ thời điểm gửi mong muốn.

### Câu 43. Vì sao service nền phải `CreateScope()`?

**Trả lời mẫu:**

Vì:

- background service sống lâu như singleton,
- nhưng `FinanceDbContext`, `ReportService`, `IEmailSender` là scoped.

Nên mỗi lần cần dùng phải:

- tạo scope mới,
- rồi resolve service scoped trong scope đó.

Muốn hiểu câu này phải phân biệt 3 lifetime trong DI:

- `Singleton`: tạo 1 lần cho toàn bộ vòng đời ứng dụng.
- `Scoped`: tạo 1 lần cho mỗi request hoặc mỗi scope.
- `Transient`: mỗi lần xin là tạo mới hoàn toàn.

Trong dự án này:

- `EmailQueue` là `Singleton`,
- `FinanceDbContext`, `ReportService`, `IEmailSender` là `Scoped`,
- `QueuedEmailBackgroundService` và `WeeklyReportHostedService` sống rất lâu, gần như kiểu singleton.

Vì background service không chạy bên trong HTTP request nên nó không có sẵn scope như controller. Do đó mỗi lần cần dùng `DbContext` hoặc service scoped, nó phải tự:

1. gọi `CreateScope()`,
2. lấy service scoped từ `scope.ServiceProvider`,
3. dùng xong thì hủy scope.

Nếu không làm vậy:

- app có thể báo lỗi kiểu “Cannot consume scoped service from singleton”,
- hoặc giữ `DbContext` quá lâu làm tracking phình ra và dễ lỗi hơn.

### Câu 44. Trong dự án này, bất đồng bộ được dùng cho những việc gì?

**Trả lời mẫu:**

Chủ yếu dùng cho:

- query database,
- ghi file,
- gửi SMTP,
- gửi SignalR,
- đọc ghi queue,
- delay của background service.

Mục đích là tránh block thread request.

Các nhóm file tiêu biểu:

- query database:
  - `AuthController.cs`
  - `DashboardController.cs`
  - `ReportService.cs`
  - `ForecastService.cs`
  - các lệnh như `FirstOrDefaultAsync`, `ToListAsync`, `SaveChangesAsync`
- gửi SMTP:
  - `SmtpEmailSender.cs`
  - dùng `SendMailAsync(...)`
- gửi SignalR:
  - `BudgetMonitorService.cs`
  - `WalletBalanceMonitorService.cs`
  - dùng `SendAsync(...)`
- đọc ghi queue:
  - `EmailQueue.cs`
  - `QueuedEmailBackgroundService.cs`
- ghi file:
  - `AvatarStorageService.cs` với `CopyToAsync(...)`
  - `SmtpEmailSender.cs` ở chế độ dev có ghi file HTML
- delay của worker:
  - `WeeklyReportHostedService.cs`
  - dùng `Task.Delay(...)`

Ý chính cần nhớ là async chủ yếu hữu ích khi phải chờ I/O như DB, file, mạng, queue. Những phép tính thuần trong RAM như cộng trừ `CurrentBalance` thì không cần async.

## 7. Câu hỏi về quản lý tài chính

### Câu 45. Tại sao `CurrentBalance` của ví được lưu sẵn trong DB?

**Trả lời mẫu:**

Vì nếu mỗi lần mở dashboard mà phải sum toàn bộ transaction thì:

- sẽ tốn chi phí query hơn,
- màn hình tổng hợp sẽ chậm hơn.

Lưu sẵn `CurrentBalance` giúp đọc nhanh hơn.

Đổi lại:

- logic tạo, sửa, xóa giao dịch phải cập nhật số dư rất cẩn thận.

`CurrentBalance` nằm trong entity `Wallet` và được hiểu là số dư hiện tại của ví sau tất cả các giao dịch đã áp dụng.

Cần phân biệt:

- `InitialBalance`: số dư lúc tạo ví,
- `CurrentBalance`: số dư hiện tại đang dùng để hiển thị dashboard, thẻ ví và kiểm tra ví thấp.

Khi tạo ví mới, hệ thống gán:

- `InitialBalance = model.InitialBalance`
- `CurrentBalance = model.InitialBalance`

Khi sửa số dư ban đầu của ví, `WalletsController` cộng hoặc trừ phần chênh lệch vào `CurrentBalance` để giữ dữ liệu nhất quán.

### Câu 46. Khi tạo giao dịch chi, số dư ví thay đổi ra sao?

**Trả lời mẫu:**

Nếu là `Expense`:

- `CurrentBalance -= Amount`

Nếu là `Income`:

- `CurrentBalance += Amount`

Logic này nằm trong `TransactionsController.ApplyWalletBalance(...)`.

Hiểu đơn giản:

- `Expense` là tiền đi ra khỏi ví nên phải trừ,
- `Income` là tiền đi vào ví nên phải cộng.

Ví dụ:

- ví đang có `1.000.000`,
- thêm giao dịch `Expense = 150.000`,
- số dư mới là `850.000`.

Nếu sau đó có giao dịch `Income = 500.000`:

- số dư sẽ thành `1.350.000`.

### Câu 47. Khi sửa giao dịch, vì sao phải reverse rồi mới apply?

**Trả lời mẫu:**

Vì transaction cũ đã từng tác động lên số dư ví.

Nếu sửa trực tiếp mà không hoàn tác:

- số dư sẽ bị cộng/trừ sai hai lần.

Cho nên quy trình đúng là:

1. reverse ảnh hưởng cũ,
2. cập nhật dữ liệu giao dịch,
3. apply ảnh hưởng mới.

### Câu 48. Vì sao xóa ví bị chặn nếu ví đã có giao dịch?

**Trả lời mẫu:**

Vì ví đã có transaction thì:

- nó gắn với lịch sử tài chính,
- xóa đi dễ làm mất tính toàn vẹn dữ liệu.

Hệ thống chọn giữ lại ví để bảo toàn lịch sử.

### Câu 49. `Budget` dùng để làm gì?

**Trả lời mẫu:**

`Budget` lưu hạn mức chi theo:

- người dùng,
- danh mục,
- tháng,
- năm.

Nó là cơ sở để hệ thống tính xem chi tiêu đã vượt định mức hay chưa.

### Câu 50. Vì sao budget phải unique theo `(UserId, CategoryId, Year, Month)`?

**Trả lời mẫu:**

Vì trong cùng một tháng, cùng một user, cùng một category chỉ nên có một mức ngân sách.

Nếu không unique:

- dữ liệu sẽ mâu thuẫn,
- không biết hệ thống phải lấy budget nào để tính cảnh báo.

Unique này được chặn ở 2 lớp:

- `FinanceDbContext` tạo unique index trong DB,
- `BudgetsController` kiểm tra trùng trước khi lưu để báo lỗi đẹp cho người dùng.

Như vậy:

- controller giúp trải nghiệm người dùng tốt hơn,
- database là lớp bảo vệ cuối cùng để đảm bảo tính toàn vẹn dữ liệu.

## 8. Câu hỏi về gợi ý danh mục và từ khóa cá nhân

### Câu 51. `UserPersonalKeyword` dùng để làm gì?

**Trả lời mẫu:**

Nó lưu:

- từ khóa người dùng thường nhập,
- danh mục mà người dùng hay gắn với từ khóa đó,
- số lần dùng,
- lần dùng gần nhất.

Mục tiêu là giúp hệ thống gợi ý danh mục ngày càng cá nhân hóa hơn.

Nó được dùng chủ yếu trong `CategorizationService`:

- `SuggestAsync(...)`: đọc lại từ khóa cá nhân để đoán category,
- `LearnAsync(...)`: học thêm khi người dùng chọn category thực tế.

Luồng hoạt động:

1. User nhập ghi chú giao dịch như “trà sữa”, “grab”, “xăng”.
2. `TransactionsController` gọi `CategorizationService.SuggestAsync(...)`.
3. Service ưu tiên tra `UserPersonalKeyword` của chính user đó.
4. Nếu không có mới rơi xuống rule chung của hệ thống.
5. Sau khi user lưu giao dịch, `LearnAsync(...)` có thể cập nhật hoặc thêm mới từ khóa cá nhân để lần sau gợi ý sát hơn.

### Câu 52. `CategorizationService` gợi ý danh mục theo những cách nào?

**Trả lời mẫu:**

Có 2 nguồn chính:

1. `personal`
   - dựa trên `UserPersonalKeyword`
2. `rule`
   - dựa trên bộ từ khóa cố định như “xăng”, “grab”, “cafe”, “lương”, “thưởng”

### Câu 53. Vì sao confidence của `personal` là `0.98` còn `rule` là `0.75`?

**Trả lời mẫu:**

Vì:

- dữ liệu `personal` phản ánh đúng lịch sử chọn của chính user,
- nên độ tin cậy được đánh giá cao hơn.

`rule` chỉ là luật suy đoán chung nên confidence thấp hơn.

Nếu đổi các mức này:

- logic cốt lõi không hỏng,
- nhưng cách hiển thị độ tin cậy cho người dùng sẽ thay đổi.

Trong source, các giá trị này được trả trực tiếp ở `CategorizationService`:

- `personal` trả `0.98`
- `rule` trả `0.75`

Ý nghĩa:

- `personal` dựa trên lịch sử thật của chính user nên được xem là gần chắc chắn hơn,
- `rule` chỉ dựa trên từ khóa chung như “xăng”, “grab”, “lương” nên có thể sai hơn.

Đây là con số heuristic do nhóm tự đặt để biểu diễn độ tin cậy, không phải xác suất AI học máy được huấn luyện thật.

## 9. Câu hỏi về cảnh báo ngân sách và các con số liên quan

### Câu 54. Cảnh báo ngân sách được kích hoạt ở mức bao nhiêu phần trăm?

**Trả lời mẫu:**

Hiện tại logic thực tế kích hoạt khi:

- `usagePercent >= 100`

Tức là chỉ khi vượt ngân sách thật sự.

Code này đang nằm trong `BudgetMonitorService.CheckAndNotifyAsync(...)`.

Service tính:

- `spentAmount`: tổng chi của category trong tháng,
- `usagePercent = spentAmount / limitAmount * 100`

Sau đó kiểm tra:

- nếu `< 100` thì không tạo `BudgetAlert`,
- nếu `>= 100` thì mới lưu alert, phát SignalR và queue email.

### Câu 55. `AlertThresholdPercent` để làm gì nếu hiện chưa được dùng đầy đủ?

**Trả lời mẫu:**

Field này thể hiện ý đồ thiết kế:

- hệ thống có thể mở rộng để cảnh báo sớm ở 80%, 90%, ...

Hiện tại:

- dữ liệu đã có,
- UI đã có,
- nhưng service chưa dùng nó để phát cảnh báo sớm.

Vì vậy nếu trên giao diện bạn chỉnh `AlertThresholdPercent = 80` thì:

- dữ liệu 80 vẫn được lưu vào `Budget`,
- nhưng hệ thống hiện vẫn cảnh báo ở `100%`,
- vì `BudgetMonitorService` chưa đọc field này để trigger alert.

Đây là một điểm phản biện rất tốt khi bảo vệ:

- thiết kế dữ liệu đã chuẩn bị cho cảnh báo sớm,
- nhưng logic nghiệp vụ hiện mới hoàn thành mức “vượt ngân sách”.

### Câu 56. Nếu sửa logic từ `100%` thành `80%` thì điều gì xảy ra?

**Trả lời mẫu:**

Nếu sửa thành `80%`:

- người dùng sẽ được nhắc sớm hơn,
- số lượng cảnh báo sẽ tăng lên,
- trải nghiệm phòng ngừa tốt hơn,
- nhưng cũng dễ tạo cảm giác “bị nhắc quá nhiều” nếu không thiết kế UI hợp lý.

### Câu 57. `BudgetAlert` lưu những thông tin gì?

**Trả lời mẫu:**

Nó lưu:

- user nào bị cảnh báo,
- budget nào bị vượt,
- transaction nào gây ra,
- message,
- số tiền đã chi,
- hạn mức,
- phần trăm sử dụng,
- đã đọc chưa,
- đã gửi email chưa.

## 10. Câu hỏi về dashboard, báo cáo và dự báo chi tiêu

### Câu 58. Dashboard trong dự án này hiển thị những gì?

**Trả lời mẫu:**

Dashboard hiển thị:

- tổng số dư,
- tổng thu và tổng chi theo tháng,
- dự báo chi cuối tháng,
- tỷ lệ tiết kiệm,
- top danh mục chi,
- ví đang có,
- giao dịch gần đây,
- cảnh báo ngân sách,
- so sánh thu chi theo nhiều tháng.

### Câu 59. Tỷ lệ tiết kiệm được tính như thế nào?

**Trả lời mẫu:**

Công thức là:

- `SavingsRate = ((Income - Expense) / Income) * 100`

Nếu `Income = 0` thì hệ thống trả `0` để tránh chia cho 0.

Code đang nằm trong `DashboardController`.

Ý nghĩa công thức:

- `Income - Expense`: phần tiền còn lại,
- chia cho `Income`: xem giữ lại được bao nhiêu phần trăm thu nhập,
- nhân `100`: đổi sang phần trăm.

Ví dụ:

- thu `10.000.000`,
- chi `7.000.000`,
- tiết kiệm `3.000.000`,
- `SavingsRate = 30%`.

### Câu 60. Dự báo chi tiêu cuối tháng được tính theo ý tưởng gì?

**Trả lời mẫu:**

Hệ thống:

1. lấy toàn bộ chi tiêu tháng hiện tại,
2. tách khoản chi thường xuyên và khoản chi đột biến,
3. nội suy phần chi thường xuyên cho cả tháng,
4. tham chiếu thêm trung bình 3 tháng gần nhất,
5. trộn lại để ra kết quả ổn định hơn.

### Câu 61. Vì sao dự báo không chỉ lấy trung bình 3 tháng trước?

**Trả lời mẫu:**

Nếu chỉ lấy lịch sử:

- sẽ không phản ánh biến động hiện tại của tháng đang diễn ra.

Nếu chỉ nhìn tháng hiện tại:

- kết quả dễ nhiễu nếu mới đi được ít ngày.

Cho nên hệ thống trộn cả 2 nguồn.

### Câu 62. Vì sao forecast dùng trọng số `0.65` và `0.35`?

**Trả lời mẫu:**

Ý tưởng là:

- ưu tiên dữ liệu tháng hiện tại hơn,
- nhưng vẫn giữ lịch sử để làm mượt kết quả.

`0.65` cho hiện tại và `0.35` cho lịch sử là một mức cân bằng.

Nếu tăng trọng số hiện tại:

- kết quả nhạy hơn với tháng đang chạy,
- nhưng dao động mạnh hơn.

Nếu tăng trọng số lịch sử:

- kết quả ổn định hơn,
- nhưng phản ứng chậm hơn với thay đổi thực tế.

### Câu 63. Tại sao forecast phải tách chi đột biến?

**Trả lời mẫu:**

Vì nếu một khoản chi bất thường rất lớn bị xem như chi tiêu thường xuyên thì:

- phần nội suy cho cả tháng sẽ bị đội lên sai lệch.

Tách đột biến giúp dự báo thực tế hơn.

### Câu 64. Tại sao `ForecastService` lại có ngưỡng tối thiểu `500000`?

**Trả lời mẫu:**

Vì nếu chỉ dựa trên IQR hoặc median:

- ở tập dữ liệu nhỏ,
- các khoản chi hơi cao hơn bình thường cũng có thể bị xem là đột biến.

Ngưỡng sàn `500000` giúp:

- chỉ những khoản đủ lớn mới bị tách riêng.

Nếu giảm sàn này:

- hệ thống nhạy hơn,
- nhưng dễ đánh dấu nhầm các khoản chi bình thường.

Nếu tăng sàn này:

- hệ thống bớt nhạy,
- nhưng có thể bỏ sót một số khoản đột biến vừa phải.

## 11. Câu hỏi về bảng dữ liệu và quan hệ

### Câu 65. `SystemLog` dùng để làm gì?

**Trả lời mẫu:**

`SystemLog` dùng để lưu:

- hành động quan trọng,
- thao tác của người dùng,
- thông điệp cần truy vết.

Admin có thể xem bảng này để kiểm soát vận hành và điều tra sự cố.

Các trường thường gặp trong `SystemLog` là:

- `Level`
- `Action`
- `Message`
- `Data`
- `CreatedAt`
- `UserId`

Trong code:

- entity nằm ở `SystemEntities.cs`,
- ghi log thông qua `AuditLogService.WriteAsync(...)`,
- admin đọc log ở `AdminController`.

Ví dụ các thao tác có ghi log:

- đăng ký, đổi mật khẩu,
- tạo, sửa, xóa giao dịch,
- tạo, sửa, xóa ví,
- thao tác quản trị.

### Câu 66. `ReportDispatchLog` dùng để làm gì?

**Trả lời mẫu:**

Nó lưu lịch sử gửi báo cáo email.

Mục tiêu:

- tránh gửi lặp cùng một báo cáo tuần cho cùng user,
- có thể truy vết user nào đã được gửi báo cáo.

Thành phần này đặc biệt quan trọng trong `WeeklyReportHostedService`.

Mỗi lần worker định gửi báo cáo tuần, nó sẽ kiểm tra:

- user đó,
- loại báo cáo đó,
- kỳ báo cáo đó

đã có bản ghi trong `ReportDispatchLog` hay chưa.

Nếu đã có thì bỏ qua để không gửi trùng.

### Câu 67. Tại sao `ReportDispatchLog` phải unique theo `(UserId, ReportType, PeriodKey)`?

**Trả lời mẫu:**

Vì tổ hợp này xác định duy nhất một lần gửi báo cáo theo kỳ.

Nếu bỏ unique:

- background service có thể gửi trùng mail trong cùng một kỳ.

### Câu 68. `BudgetAlert` liên kết với những bảng nào?

**Trả lời mẫu:**

`BudgetAlert` liên kết với:

- `AppUser`
- `Budget`
- `WalletTransaction`

Điều này giúp biết:

- cảnh báo thuộc user nào,
- budget nào bị vượt,
- giao dịch nào là nguyên nhân trực tiếp.

### Câu 69. `UserPersonalKeyword` liên kết với những bảng nào?

**Trả lời mẫu:**

Nó liên kết với:

- `AppUser`
- `Category`

Nghĩa là từ khóa đó là của user nào, và hệ thống gắn từ khóa đó với category nào.

## 12. Câu hỏi về phần quản trị

### Câu 70. Dashboard admin dùng để theo dõi gì?

**Trả lời mẫu:**

Dashboard admin theo dõi:

- tổng số user,
- số user đang active,
- tổng số transaction,
- user mới trong ngày,
- tổng số category,
- log gần đây,
- user mới gần đây,
- thống kê theo ngày.

### Câu 71. Vì sao admin không được khóa chính tài khoản admin từ màn hình user?

**Trả lời mẫu:**

Vì nếu khóa nhầm admin:

- hệ thống có thể mất quyền quản trị,
- gây khó khăn cho vận hành.

Đây là một ràng buộc an toàn.

### Câu 72. Vì sao category đã có transaction hoặc budget thì không cho đổi loại hoặc xóa?

**Trả lời mẫu:**

Vì nếu category đã được dùng mà vẫn đổi loại hoặc xóa:

- dữ liệu lịch sử sẽ mất tính nhất quán,
- báo cáo và transaction cũ có thể sai nghĩa.

## 13. Câu hỏi phản biện nâng cao

### Câu 73. Hạn chế hiện tại của `EmailQueue` là gì?

**Trả lời mẫu:**

`EmailQueue` đang là in-memory queue.

Hệ quả:

- nếu app restart thì mail chưa gửi có thể mất,
- khó scale tốt khi triển khai nhiều instance.

Giải pháp nâng cấp:

- dùng RabbitMQ,
- Azure Queue,
- Hangfire,
- hoặc một hệ thống hàng đợi bền vững khác.

### Câu 74. Hạn chế hiện tại của logic cảnh báo ngân sách là gì?

**Trả lời mẫu:**

Hệ thống hiện mới cảnh báo khi vượt 100%.

Trong khi đó:

- field `AlertThresholdPercent` đã có,
- nhưng chưa dùng để phát cảnh báo sớm.

### Câu 75. Hạn chế hiện tại của dashboard là gì?

**Trả lời mẫu:**

`DashboardController` đang nạp khá nhiều transaction vào memory rồi mới tính toán.

Điều này ổn với demo hoặc dữ liệu vừa phải.

Nhưng nếu dữ liệu lớn:

- cần tối ưu query hơn,
- đẩy nhiều phép tổng hợp xuống database.

### Câu 76. Nếu muốn tăng độ an toàn OTP, em sẽ cải tiến gì?

**Trả lời mẫu:**

Có thể cải tiến theo các hướng:

- thêm giới hạn gửi OTP theo thời gian,
- thêm cooldown giữa 2 lần gửi,
- thêm rate limit theo IP,
- giảm thời hạn OTP,
- tăng độ dài OTP,
- lưu thêm log gửi OTP để giám sát.

### Câu 77. Nếu muốn mở rộng cảnh báo thời gian thực, em sẽ làm gì?

**Trả lời mẫu:**

Em có thể mở rộng bằng cách:

- thêm event cho báo cáo đã sẵn sàng,
- thêm event khi gửi mail thất bại,
- thêm event cho cảnh báo sắp chạm ngân sách,
- đồng bộ badge số lượng cảnh báo chưa đọc realtime trên mọi trang.

## 14. Câu hỏi nhanh kiểu giáo viên hay hỏi chốt

### Câu 78. Nêu ngắn gọn luồng đăng ký tài khoản.

**Trả lời mẫu:**

Người dùng nhập form đăng ký, hệ thống kiểm tra trùng email/username, băm mật khẩu, lưu user ở trạng thái chưa xác thực, sinh OTP, đưa email OTP vào queue, người dùng nhập OTP đúng thì `IsEmailVerified = true`, sau đó mới đăng nhập được.

### Câu 79. Nêu ngắn gọn luồng tạo giao dịch chi.

**Trả lời mẫu:**

Controller validate dữ liệu, kiểm tra ví và category, gọi gợi ý danh mục, tạo transaction, trừ số dư ví, lưu DB, học từ khóa cá nhân, kiểm tra ngân sách, phát cảnh báo realtime nếu cần, ghi log.

### Câu 80. Nêu ngắn gọn luồng gửi báo cáo tuần tự động.

**Trả lời mẫu:**

Background service chạy định kỳ, đến đúng khung giờ thì lấy danh sách user hợp lệ, kiểm tra đã gửi kỳ đó chưa, tạo nội dung báo cáo, đưa email vào queue, ghi `ReportDispatchLog`, sau đó worker email sẽ gửi mail thực tế.

## 15. Gợi ý cách học tài liệu này

Bạn nên học theo thứ tự:

1. Kiến trúc tổng quan.
2. Luồng đăng nhập, OTP, cookie.
3. Luồng tạo giao dịch và cập nhật số dư ví.
4. Luồng cảnh báo ngân sách và SignalR.
5. Luồng báo cáo, Excel và email.
6. Luồng background service và forecast.
7. Câu hỏi phản biện về tham số và hạn chế.
