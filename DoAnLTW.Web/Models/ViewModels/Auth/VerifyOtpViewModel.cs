using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class VerifyOtpViewModel
{
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Ma OTP")]
    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    public string Purpose { get; set; } = "Register";
}
