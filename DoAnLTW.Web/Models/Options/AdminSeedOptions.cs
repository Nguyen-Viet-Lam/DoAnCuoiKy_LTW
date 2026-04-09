namespace DoAnLTW.Web.Models.Options;

/// <summary>Class cấu hình thông tin tài khoản admin mặc định dùng cho quá trình seed dữ liệu.</summary>
public class AdminSeedOptions
{
    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    public string Email { get; set; } = "admin@financeflow.local";
    /// <summary>
    /// Tên đăng nhập dùng để nhận diện tài khoản trong hệ thống.
    /// </summary>
    public string Username { get; set; } = "admin";
    /// <summary>
    /// Tên hiển thị của người dùng trên giao diện.
    /// </summary>
    public string DisplayName { get; set; } = "Quan tri vien";
    /// <summary>
    /// Mật khẩu người dùng nhập vào hoặc mật khẩu cấu hình cần sử dụng.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
