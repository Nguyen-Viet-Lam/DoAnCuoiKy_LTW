using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form đăng ký tài khoản mới.</summary>
public class RegisterViewModel
{
    /// <summary>
    /// Tên đăng nhập dùng để nhận diện tài khoản trong hệ thống.
    /// </summary>
    [Display(Name = "Ten dang nhap")]
    [Required, StringLength(64)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị của người dùng trên giao diện.
    /// </summary>
    [Display(Name = "Ten hien thi")]
    [Required, StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu người dùng nhập vào hoặc mật khẩu cấu hình cần sử dụng.
    /// </summary>
    [Display(Name = "Mat khau")]
    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu nhập lại để kiểm tra người dùng gõ trùng khớp.
    /// </summary>
    [Display(Name = "Nhap lai mat khau")]
    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
