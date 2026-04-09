namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model mô tả một ví trong danh sách quản lý ví.</summary>
public class WalletItemViewModel
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Tên hiển thị của ví hoặc nguồn tiền.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Loại ví hoặc nguồn tiền, ví dụ tiền mặt hay ngân hàng.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Số dư hiện tại của ví sau khi áp dụng các giao dịch.
    /// </summary>
    public decimal CurrentBalance { get; set; }
    /// <summary>
    /// Ghi chú bổ sung cho ví hoặc nguồn tiền.
    /// </summary>
    public string? Note { get; set; }
    /// <summary>
    /// Tổng số giao dịch liên quan đến bản ghi hoặc thống kê hiện tại.
    /// </summary>
    public int TransactionCount { get; set; }
    /// <summary>
    /// Cho biết số dư hiện tại có đang dưới ngưỡng cảnh báo ví thấp hay không.
    /// </summary>
    public bool IsLowBalance { get; set; }
}
