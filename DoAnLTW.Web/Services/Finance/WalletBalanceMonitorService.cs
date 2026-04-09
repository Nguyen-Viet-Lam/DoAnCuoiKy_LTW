using DoAnLTW.Web.Hubs;
using DoAnLTW.Web.Models.Entities;
using Microsoft.AspNetCore.SignalR;

namespace DoAnLTW.Web.Services.Finance;

/// <summary>Service phát hiện ví vừa xuống dưới ngưỡng số dư thấp và gửi cảnh báo realtime.</summary>
public class WalletBalanceMonitorService
{
    /// <summary>
    /// Ngưỡng số dư thấp dùng để xác định khi nào cần phát cảnh báo ví.
    /// </summary>
    public const decimal LowBalanceThreshold = 200000m;

    private readonly IHubContext<BudgetHub> _hubContext;

    /// <summary>
    /// Khởi tạo lớp WalletBalanceMonitorService và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public WalletBalanceMonitorService(IHubContext<BudgetHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Kiểm tra số dư hiện tại có đang thấp hơn ngưỡng cảnh báo ví thấp hay không.
    /// </summary>
    public bool IsLowBalance(decimal balance)
    {
        return balance < LowBalanceThreshold;
    }

    /// <summary>
    /// Phát cảnh báo realtime khi số dư ví vừa giảm xuống dưới ngưỡng cấu hình.
    /// </summary>
    public async Task NotifyIfThresholdCrossedAsync(
        int userId,
        Wallet wallet,
        decimal previousBalance,
        CancellationToken cancellationToken = default)
    {
        if (previousBalance < LowBalanceThreshold || wallet.CurrentBalance >= LowBalanceThreshold)
        {
            return;
        }

        var message = $"Vi {wallet.Name} da xuong duoi nguong {LowBalanceThreshold:N0} VND.";

        await _hubContext.Clients.Group($"user-{userId}")
            .SendAsync("walletAlert", new
            {
                walletName = wallet.Name,
                currentBalance = wallet.CurrentBalance,
                threshold = LowBalanceThreshold,
                message
            }, cancellationToken);
    }
}
