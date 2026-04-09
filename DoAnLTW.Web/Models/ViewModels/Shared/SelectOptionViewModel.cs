namespace DoAnLTW.Web.Models.ViewModels;

/// <summary>View model dùng chung cho các option trong dropdown chọn ví, danh mục hoặc bộ lọc.</summary>
public class SelectOptionViewModel
{
    /// <summary>
    /// Giá trị thật của một lựa chọn trong dropdown hoặc danh sách chọn.
    /// </summary>
    public int Value { get; set; }
    /// <summary>
    /// Nhãn hiển thị ra giao diện cho một lựa chọn.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>
    /// Thông tin phụ đi kèm lựa chọn, ví dụ màu sắc hoặc dữ liệu hiển thị thêm.
    /// </summary>
    public string? Extra { get; set; }
}
