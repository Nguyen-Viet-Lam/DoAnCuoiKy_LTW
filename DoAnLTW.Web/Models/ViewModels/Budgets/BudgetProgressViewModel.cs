namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model mô tả tiến độ sử dụng của một ngân sách theo danh mục và tháng.</summary>
public class BudgetProgressViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Tên danh mục được hiển thị cùng giao dịch hoặc thống kê.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    /// <summary>
    /// Mã màu hiển thị dùng cho danh mục, thẻ hoặc biểu đồ.
    /// </summary>
    public string ColorHex { get; set; } = "#8ecae6";
    /// <summary>
    /// Hạn mức chi tối đa được đặt ra cho kỳ ngân sách.
    /// </summary>
    public decimal LimitAmount { get; set; }
    /// <summary>
    /// Tổng số tiền đã chi trong kỳ ngân sách đang xét.
    /// </summary>
    public decimal SpentAmount { get; set; }
    /// <summary>
    /// Tỷ lệ sử dụng ngân sách hoặc mức độ hoàn thành, tính theo phần trăm.
    /// </summary>
    public double UsagePercent { get; set; }
    /// <summary>
    /// Nhãn tháng dùng để hiển thị thân thiện trên giao diện.
    /// </summary>
    public string MonthLabel { get; set; } = string.Empty;
    /// <summary>
    /// Ngưỡng phần trăm cảnh báo được cấu hình cho ngân sách.
    /// </summary>
    public int AlertThresholdPercent { get; set; }
}
