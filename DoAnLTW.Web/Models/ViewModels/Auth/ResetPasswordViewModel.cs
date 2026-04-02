using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class ResetPasswordViewModel
{
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Ma OTP")]
    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "Mat khau moi")]
    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Nhap lai mat khau moi")]
    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
