using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho form đăng nhập của người dùng.</summary>
public class LoginViewModel
{
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
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Cho biết người dùng có muốn duy trì đăng nhập lâu hơn hay không.
    /// </summary>
    [Display(Name = "Ghi nho dang nhap")]
    public bool RememberMe { get; set; }

    /// <summary>
    /// Đường dẫn cần quay lại sau khi người dùng đăng nhập thành công.
    /// </summary>
    public string? ReturnUrl { get; set; }
}
