using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class ForgotPasswordViewModel
{
    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
