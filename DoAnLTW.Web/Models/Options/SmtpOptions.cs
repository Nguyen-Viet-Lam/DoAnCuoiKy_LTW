namespace DoAnLTW.Web.Models.Options;

/// <summary>Class cấu hình SMTP dùng cho việc gửi email OTP, cảnh báo và báo cáo.</summary>
public class SmtpOptions
{
    /// <summary>
    /// Tên máy chủ SMTP dùng để gửi email ra ngoài.
    /// </summary>
    public string Host { get; set; } = "smtp.gmail.com";
    /// <summary>
    /// Cổng kết nối tới máy chủ SMTP.
    /// </summary>
    public int Port { get; set; } = 587;
    /// <summary>
    /// Cho biết kết nối SMTP có bật mã hóa SSL/TLS hay không.
    /// </summary>
    public bool EnableSsl { get; set; } = true;
    /// <summary>
    /// Tên đăng nhập dùng để nhận diện tài khoản trong hệ thống.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>
    /// Mật khẩu hoặc app password dùng để xác thực với máy chủ SMTP.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// Địa chỉ email người gửi hiển thị trên thư đi.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;
    /// <summary>
    /// Tên người gửi hiển thị trên email.
    /// </summary>
    public string FromName { get; set; } = "Finance Flow";
}
