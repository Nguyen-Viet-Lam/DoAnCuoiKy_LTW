namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model mô tả một tài khoản người dùng trong danh sách quản trị.</summary>
public class AdminUserItemViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Tên đăng nhập dùng để nhận diện tài khoản trong hệ thống.
    /// </summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>
    /// Tên hiển thị của người dùng trên giao diện.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>
    /// Địa chỉ email dùng cho đăng nhập, OTP hoặc nhận báo cáo.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Tên vai trò hiển thị của người dùng trên màn hình quản trị.
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    /// <summary>
    /// Cho biết đối tượng hoặc tài khoản này còn được phép sử dụng hay không.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Cho biết email của người dùng đã được xác thực OTP hay chưa.
    /// </summary>
    public bool IsEmailVerified { get; set; }
    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Tổng số ví liên quan đến người dùng hoặc thống kê hiện tại.
    /// </summary>
    public int WalletCount { get; set; }
    /// <summary>
    /// Tổng số giao dịch liên quan đến bản ghi hoặc thống kê hiện tại.
    /// </summary>
    public int TransactionCount { get; set; }
    /// <summary>
    /// Tổng số dư cộng dồn của các ví đang được hiển thị.
    /// </summary>
    public decimal TotalBalance { get; set; }
}
