using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class RegisterViewModel
{
    [Display(Name = "Ten dang nhap")]
    [Required, StringLength(64)]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Ten hien thi")]
    [Required, StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Mat khau")]
    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Nhap lai mat khau")]
    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
