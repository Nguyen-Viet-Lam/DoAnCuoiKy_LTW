namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model cho thẻ ví rút gọn hiển thị trên dashboard.</summary>
public class WalletMiniCardViewModel
{
    /// <summary>
    /// Tên hiển thị của ví hoặc nguồn tiền.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Loại ví hoặc nguồn tiền, ví dụ tiền mặt hay ngân hàng.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Số dư hoặc giá trị tiền hiện tại cần hiển thị.
    /// </summary>
    public decimal Balance { get; set; }
    /// <summary>
    /// Cho biết số dư hiện tại có đang dưới ngưỡng cảnh báo ví thấp hay không.
    /// </summary>
    public bool IsLowBalance { get; set; }
}
