namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model tổng hợp dữ liệu cho trang báo cáo tuần, tháng và khoảng ngày tùy chọn.</summary>
public class ReportsPageViewModel
{
    /// <summary>
    /// Bộ điều kiện lọc mà trang hiện tại đang sử dụng.
    /// </summary>
    public ReportFilterViewModel Filter { get; set; } = new();
    /// <summary>
    /// Bản tóm tắt báo cáo cho khoảng 7 ngày gần nhất.
    /// </summary>
    public ReportSummaryViewModel WeeklySummary { get; set; } = new();
    /// <summary>
    /// Bản tóm tắt báo cáo cho tháng hiện tại.
    /// </summary>
    public ReportSummaryViewModel MonthlySummary { get; set; } = new();
    /// <summary>
    /// Bản tóm tắt báo cáo cho khoảng ngày người dùng tự chọn.
    /// </summary>
    public ReportSummaryViewModel CustomSummary { get; set; } = new();
    /// <summary>
    /// Địa chỉ email mặc định gợi ý khi gửi báo cáo.
    /// </summary>
    public string DefaultEmail { get; set; } = string.Empty;
}
