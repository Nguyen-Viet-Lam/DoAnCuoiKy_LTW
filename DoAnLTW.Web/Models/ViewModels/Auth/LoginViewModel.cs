using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class LoginViewModel
{
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Mat khau")]
    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nho dang nhap")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
