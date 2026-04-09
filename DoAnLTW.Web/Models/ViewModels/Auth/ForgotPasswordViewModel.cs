using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form yêu cầu gửi OTP khi người dùng quên mật khẩu.</summary>
public class ForgotPasswordViewModel
{
    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
