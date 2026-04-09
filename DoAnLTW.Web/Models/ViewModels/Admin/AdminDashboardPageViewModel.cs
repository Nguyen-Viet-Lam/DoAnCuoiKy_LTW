namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp các chỉ số, log và người dùng mới trên dashboard admin.</summary>
public class AdminDashboardPageViewModel
{
    /// <summary>
    /// Tổng số người dùng dùng để hiển thị thống kê quản trị.
    /// </summary>
    public int TotalUsers { get; set; }
    /// <summary>
    /// Số lượng người dùng đang ở trạng thái hoạt động.
    /// </summary>
    public int ActiveUsers { get; set; }
    /// <summary>
    /// Tổng số giao dịch đã được ghi nhận trong hệ thống.
    /// </summary>
    public int TotalTransactions { get; set; }
    /// <summary>
    /// Số lượng tài khoản mới tạo trong ngày hiện tại.
    /// </summary>
    public int NewUsersToday { get; set; }
    /// <summary>
    /// Tổng số danh mục đang có trong hệ thống.
    /// </summary>
    public int TotalCategories { get; set; }
    /// <summary>
    /// Dữ liệu thống kê theo ngày dùng cho biểu đồ quản trị.
    /// </summary>
    public List<AdminDailyStatViewModel> DailyStats { get; set; } = [];
    /// <summary>
    /// Danh sách tài khoản mới tạo gần đây trên hệ thống.
    /// </summary>
    public List<AdminUserItemViewModel> LatestUsers { get; set; } = [];
    /// <summary>
    /// Danh sách log mới nhất để quản trị viên theo dõi nhanh.
    /// </summary>
    public List<SystemLogRowViewModel> LatestLogs { get; set; } = [];
}
