using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form xác thực OTP sau đăng ký hoặc khi xử lý bảo mật.</summary>
public class VerifyOtpViewModel
{
    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mã OTP người dùng nhập để xác thực email hoặc thao tác bảo mật.
    /// </summary>
    [Display(Name = "Ma OTP")]
    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mục đích sử dụng của OTP như đăng ký hoặc đặt lại mật khẩu.
    /// </summary>
    public string Purpose { get; set; } = "Register";
}
