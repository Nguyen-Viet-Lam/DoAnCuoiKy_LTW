using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

public class ProfileViewModel
{
    [Display(Name = "Ten hien thi")]
    [Required, StringLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? AvatarPath { get; set; }

    [Display(Name = "Anh dai dien")]
    public IFormFile? AvatarFile { get; set; }

    [Display(Name = "Mat khau hien tai")]
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [Display(Name = "Mat khau moi")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [Display(Name = "Nhap lai mat khau moi")]
    [DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string? ConfirmPassword { get; set; }

    public DateTime CreatedAt { get; set; }
}
