using System.ComponentModel.DataAnnotations;

namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho trang hồ sơ cá nhân gồm thông tin hiển thị, avatar và đổi mật khẩu.</summary>
public class ProfileViewModel
{
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
    /// Đường dẫn ảnh đại diện đang lưu trên máy chủ.
    /// </summary>
    public string? AvatarPath { get; set; }

    /// <summary>
    /// Tệp ảnh đại diện mới người dùng tải lên.
    /// </summary>
    [Display(Name = "Anh dai dien")]
    public IFormFile? AvatarFile { get; set; }

    /// <summary>
    /// Mật khẩu hiện tại dùng để xác thực trước khi đổi mật khẩu.
    /// </summary>
    [Display(Name = "Mat khau hien tai")]
    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    /// <summary>
    /// Mật khẩu mới mà người dùng muốn cập nhật.
    /// </summary>
    [Display(Name = "Mat khau moi")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    /// <summary>
    /// Mật khẩu nhập lại để kiểm tra người dùng gõ trùng khớp.
    /// </summary>
    [Display(Name = "Nhap lai mat khau moi")]
    [DataType(DataType.Password), Compare(nameof(NewPassword))]
    public string? ConfirmPassword { get; set; }

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
