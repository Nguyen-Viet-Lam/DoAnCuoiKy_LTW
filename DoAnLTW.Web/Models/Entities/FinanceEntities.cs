using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLTW.Web.Models.Entities;

/// <summary>Entity đại diện cho một ví hoặc nguồn tiền của người dùng.</summary>
public class Wallet
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Tên hiển thị của ví hoặc nguồn tiền.
    /// </summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại ví hoặc nguồn tiền, ví dụ tiền mặt hay ngân hàng.
    /// </summary>
    [Required, MaxLength(32)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Số dư ban đầu khi tạo ví.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal InitialBalance { get; set; }

    /// <summary>
    /// Số dư hiện tại của ví sau khi áp dụng các giao dịch.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// Ghi chú bổ sung cho ví hoặc nguồn tiền.
    /// </summary>
    [MaxLength(250)]
    public string? Note { get; set; }

    /// <summary>
    /// Cho biết ví đã bị lưu trữ hoặc ẩn khỏi danh sách sử dụng hay chưa.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Danh sách giao dịch liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

/// <summary>Entity mô tả danh mục thu chi dùng để phân loại giao dịch và ngân sách.</summary>
public class Category
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Tên danh mục hiển thị cho người dùng.
    /// </summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Loại danh mục áp dụng cho giao dịch thu hoặc chi.
    /// </summary>
    [Required, MaxLength(16)]
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Tên icon dùng để minh họa trên giao diện.
    /// </summary>
    [MaxLength(64)]
    public string Icon { get; set; } = "bi-circle";

    /// <summary>
    /// Mã màu hiển thị dùng cho danh mục, thẻ hoặc biểu đồ.
    /// </summary>
    [MaxLength(16)]
    public string ColorHex { get; set; } = "#7fc8f8";

    /// <summary>
    /// Cho biết đây có phải danh mục mặc định của hệ thống hay không.
    /// </summary>
    public bool IsDefault { get; set; } = true;

    /// <summary>
    /// Mã người dùng đã tạo bản ghi hoặc danh mục này.
    /// </summary>
    public int? CreatedByUserId { get; set; }

    /// <summary>
    /// Người dùng đã tạo danh mục này nếu đây không phải danh mục hệ thống mặc định.
    /// </summary>
    [ForeignKey(nameof(CreatedByUserId))]
    public AppUser? CreatedByUser { get; set; }

    /// <summary>
    /// Danh sách giao dịch liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();

    /// <summary>
    /// Danh sách ngân sách liên quan đến đối tượng hoặc trang hiện tại.
    /// </summary>
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}

/// <summary>Entity lưu một giao dịch thu hoặc chi phát sinh trên ví của người dùng.</summary>
public class WalletTransaction
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Mã ví được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    public int WalletId { get; set; }

    /// <summary>
    /// Mã danh mục được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Loại ví hoặc nguồn tiền, ví dụ tiền mặt hay ngân hàng.
    /// </summary>
    [Required, MaxLength(16)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Số tiền của giao dịch, thống kê hoặc mục dữ liệu tương ứng.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Ghi chú mô tả nội dung giao dịch.
    /// </summary>
    [MaxLength(300)]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Ngày giao dịch thực tế phát sinh.
    /// </summary>
    public DateTime OccurredOn { get; set; } = DateTime.Now;

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mã danh mục mà bộ gợi ý đề xuất từ ghi chú giao dịch.
    /// </summary>
    public int? AiSuggestedCategoryId { get; set; }

    /// <summary>
    /// Tên hoặc nhãn danh mục mà bộ gợi ý trả về.
    /// </summary>
    [MaxLength(100)]
    public string? AiSuggestedLabel { get; set; }

    /// <summary>
    /// Độ tin cậy của kết quả gợi ý danh mục.
    /// </summary>
    public double AiConfidence { get; set; }

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Đối tượng ví liên kết với giao dịch hoặc dữ liệu hiện tại.
    /// </summary>
    [ForeignKey(nameof(WalletId))]
    public Wallet Wallet { get; set; } = null!;

    /// <summary>
    /// Đối tượng danh mục liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}

/// <summary>Entity lưu hạn mức ngân sách theo danh mục và theo tháng.</summary>
public class Budget
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// Mã danh mục được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Năm áp dụng của ngân sách, thống kê hoặc khoảng dữ liệu.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Tháng áp dụng của ngân sách, thống kê hoặc khoảng dữ liệu.
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Hạn mức chi tối đa được đặt ra cho kỳ ngân sách.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LimitAmount { get; set; }

    /// <summary>
    /// Ngưỡng phần trăm cảnh báo được cấu hình cho ngân sách.
    /// </summary>
    public int AlertThresholdPercent { get; set; } = 80;

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Đối tượng danh mục liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Danh sách cảnh báo ngân sách liên quan.
    /// </summary>
    public ICollection<BudgetAlert> Alerts { get; set; } = new List<BudgetAlert>();
}

/// <summary>Entity lưu từ khóa cá nhân để hệ thống học cách gợi ý danh mục cho từng người dùng.</summary>
public class UserPersonalKeyword
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// Mã danh mục được chọn hoặc được liên kết với dữ liệu hiện tại.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Từ khóa mà hệ thống học được từ ghi chú để gợi ý danh mục.
    /// </summary>
    [Required, MaxLength(100)]
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// Số lần từ khóa này đã được ghi nhận trong quá trình học.
    /// </summary>
    public int HitCount { get; set; } = 1;

    /// <summary>
    /// Thời điểm gần nhất từ khóa này được sử dụng hoặc cập nhật.
    /// </summary>
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Đối tượng danh mục liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;
}

/// <summary>Entity lưu cảnh báo khi mức chi tiêu đã vượt ngân sách của một danh mục.</summary>
public class BudgetAlert
{
    /// <summary>
    /// Mã định danh của bản ghi hoặc đối tượng.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Mã người dùng sở hữu dữ liệu hoặc thao tác liên quan.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// Mã ngân sách liên quan đến cảnh báo hoặc bản ghi hiện tại.
    /// </summary>
    public int BudgetId { get; set; }

    /// <summary>
    /// Mã giao dịch gắn với cảnh báo ngân sách này.
    /// </summary>
    public int WalletTransactionId { get; set; }

    /// <summary>
    /// Nội dung thông báo, cảnh báo hoặc log được hiển thị cho người đọc.
    /// </summary>
    [MaxLength(300)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Tổng số tiền đã chi trong kỳ ngân sách đang xét.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal SpentAmount { get; set; }

    /// <summary>
    /// Hạn mức chi tối đa được đặt ra cho kỳ ngân sách.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LimitAmount { get; set; }

    /// <summary>
    /// Tỷ lệ sử dụng ngân sách hoặc mức độ hoàn thành, tính theo phần trăm.
    /// </summary>
    public double UsagePercent { get; set; }

    /// <summary>
    /// Cho biết cảnh báo đã được người dùng đọc hay chưa.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Cho biết email cảnh báo hoặc email liên quan đã được gửi hay chưa.
    /// </summary>
    public bool IsEmailSent { get; set; }

    /// <summary>
    /// Thời điểm bản ghi được tạo trong hệ thống.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Đối tượng người dùng liên kết với bản ghi hiện tại.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    /// <summary>
    /// Ngân sách gốc mà cảnh báo này đang tham chiếu tới.
    /// </summary>
    [ForeignKey(nameof(BudgetId))]
    public Budget Budget { get; set; } = null!;

    /// <summary>
    /// Giao dịch chi đã làm phát sinh cảnh báo vượt ngân sách này.
    /// </summary>
    [ForeignKey(nameof(WalletTransactionId))]
    public WalletTransaction WalletTransaction { get; set; } = null!;
}
