namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho trang xem và lọc log hệ thống trong khu vực quản trị.</summary>
public class AdminLogsPageViewModel
{
    /// <summary>
    /// Bộ lọc mức log đang áp dụng trên trang quản trị log.
    /// </summary>
    public string LevelFilter { get; set; } = "All";
    /// <summary>
    /// Từ khóa tìm kiếm hiện tại do người dùng nhập.
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;
    /// <summary>
    /// Tổng số dòng log đang có trong hệ thống.
    /// </summary>
    public int TotalLogs { get; set; }
    /// <summary>
    /// Số dòng log có mức lỗi để quản trị viên theo dõi nhanh.
    /// </summary>
    public int ErrorLogs { get; set; }
    /// <summary>
    /// Số log gắn với hành động của người dùng cụ thể.
    /// </summary>
    public int UserActionLogs { get; set; }
    /// <summary>
    /// Danh sách log đã được lọc để hiển thị cho quản trị viên.
    /// </summary>
    public List<SystemLogRowViewModel> Logs { get; set; } = [];
}
