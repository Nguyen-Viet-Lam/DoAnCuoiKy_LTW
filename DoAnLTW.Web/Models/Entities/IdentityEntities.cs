using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Web.Models.Entities;

/// <summary>Entity lưu vai trò người dùng như Admin hoặc User.</summary>
public class Role
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Tên vai trò dùng trong phân quyền như Admin hoặc User.
    /// </summary>
    [Required, MaxLength(32)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách người dùng liên kết với vai trò hoặc màn hình hiện tại.
    /// </summary>
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
}

/// <summary>Entity lưu thông tin tài khoản người dùng và các quan hệ tài chính liên quan.</summary>
public class AppUser
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Tên đăng nhập dùng để nhận diện tài khoản trong hệ thống.
    /// </summary>
    [Required, MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Tên hiển thị của người dùng trên giao diện.
    /// </summary>
    [Required, MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Chuỗi băm mật khẩu được lưu trong cơ sở dữ liệu thay vì mật khẩu gốc.
    /// </summary>
    [Required, MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Đường dẫn ảnh đại diện đang lưu trên máy chủ.
    /// </summary>
    [MaxLength(256)]
    public string? AvatarPath { get; set; }

    /// <summary>
    /// Cho biết đối tượng hoặc tài khoản này còn được phép sử dụng hay không.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Cho biết email của người dùng đã được xác thực OTP hay chưa.
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// Số điện thoại của người dùng nếu đã khai báo.
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mã vai trò được gán cho người dùng.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Đối tượng vai trò liên kết để xác định quyền của người dùng.
    /// </summary>
    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Danh sách ví liên quan dùng để hiển thị hoặc điều hướng dữ liệu.
    /// </summary>
    public ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();

    /// <summary>
    /// Danh sách giao dịch liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();

    /// <summary>
    /// Danh sách ngân sách liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    /// <summary>
    /// Danh sách từ khóa cá nhân mà hệ thống đã học từ người dùng này.
    /// </summary>
    public ICollection<UserPersonalKeyword> PersonalKeywords { get; set; } = new List<UserPersonalKeyword>();

    /// <summary>
    /// Danh sách cảnh báo ngân sách liên quan.
    /// </summary>
    public ICollection<BudgetAlert> Alerts { get; set; } = new List<BudgetAlert>();
}

/// <summary>Entity lưu mã OTP đã băm để phục vụ xác thực email và đặt lại mật khẩu.</summary>
public class EmailOtp
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    [MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mục đích sử dụng của OTP như đăng ký hoặc đặt lại mật khẩu.
    /// </summary>
    [MaxLength(32)]
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị băm của OTP để hệ thống không lưu mã gốc trực tiếp.
    /// </summary>
    [MaxLength(256)]
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>
    /// Thời điểm dữ liệu tạm như OTP sẽ hết hiệu lực.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Đánh dấu OTP này đã được dùng thành công hay chưa.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Số lần người dùng đã thử nhập OTP.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    public AppUser? User { get; set; }
}
