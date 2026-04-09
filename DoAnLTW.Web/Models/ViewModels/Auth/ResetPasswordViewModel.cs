using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form nhập OTP và đặt lại mật khẩu mới.</summary>
public class ResetPasswordViewModel
{
    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mã OTP dùng để chứng minh người dùng có quyền đặt lại mật khẩu.
    /// </summary>
    [Display(Name = "Ma OTP")]
    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu mới mà người dùng muốn cập nhật.
    /// </summary>
    [Display(Name = "Mat khau moi")]
    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu nhập lại để kiểm tra người dùng gõ trùng khớp.
    /// </summary>
    [Display(Name = "Nhap lai mat khau moi")]
    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
