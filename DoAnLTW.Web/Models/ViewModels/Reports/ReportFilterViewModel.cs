namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model lưu khoảng ngày dùng để lọc dữ liệu báo cáo.</summary>
public class ReportFilterViewModel
{
    /// <summary>
    /// Ngày bắt đầu của khoảng lọc hoặc kỳ báo cáo.
    /// </summary>
    public DateTime FromDate { get; set; }
    /// <summary>
    /// Ngày kết thúc của khoảng lọc hoặc kỳ báo cáo.
    /// </summary>
    public DateTime ToDate { get; set; }
}
