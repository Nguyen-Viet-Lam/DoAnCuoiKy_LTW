namespace DoAnLTW.Web.Services.Email;

/// <summary>DTO đóng gói thông tin một email cần gửi gồm người nhận, tiêu đề và nội dung HTML.</summary>
public class EmailMessage
{
    /// <summary>
    /// Địa chỉ email người nhận.
    /// </summary>
    public string To { get; set; } = string.Empty;
    /// <summary>
    /// Tiêu đề của email sẽ được gửi.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>
    /// Nội dung HTML của email để render đẹp trên hộp thư.
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;
}
