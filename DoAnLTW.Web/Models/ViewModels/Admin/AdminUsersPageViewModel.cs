namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp bộ lọc và danh sách tài khoản trên trang quản trị người dùng.</summary>
public class AdminUsersPageViewModel
{
    /// <summary>
    /// Từ khóa tìm kiếm hiện tại do người dùng nhập.
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;
    /// <summary>
    /// Bộ lọc trạng thái đang áp dụng trên danh sách.
    /// </summary>
    public string StatusFilter { get; set; } = "All";
    /// <summary>
    /// Bộ lọc vai trò đang áp dụng trên danh sách người dùng.
    /// </summary>
    public string RoleFilter { get; set; } = "All";
    /// <summary>
    /// Tổng số người dùng dùng để hiển thị thống kê quản trị.
    /// </summary>
    public int TotalUsers { get; set; }
    /// <summary>
    /// Số lượng người dùng đang ở trạng thái hoạt động.
    /// </summary>
    public int ActiveUsers { get; set; }
    /// <summary>
    /// Số lượng tài khoản hiện đang bị khóa hoặc vô hiệu hóa.
    /// </summary>
    public int LockedUsers { get; set; }
    /// <summary>
    /// Danh sách người dùng liên kết với vai trò hoặc màn hình hiện tại.
    /// </summary>
    public List<AdminUserItemViewModel> Users { get; set; } = [];
}
