using DoAnLTW.Web.Hubs;
using DoAnLTW.Web.Models.Entities;
using Microsoft.AspNetCore.SignalR;

namespace DoAnLTW.Web.Services.Finance;

public class WalletBalanceMonitorService
{
    public const decimal LowBalanceThreshold = 200000m;

    private readonly IHubContext<BudgetHub> _hubContext;

    public WalletBalanceMonitorService(IHubContext<BudgetHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public bool IsLowBalance(decimal balance)
    {
        return balance < LowBalanceThreshold;
    }

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
