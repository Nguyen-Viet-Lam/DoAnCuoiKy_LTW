using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DoAnLTW.Web.Hubs;

/// <summary>SignalR hub dùng để đẩy cảnh báo ngân sách và số dư ví theo thời gian thực.</summary>
[Authorize]
public class BudgetHub : Hub
{
    /// <summary>
    /// Đưa kết nối SignalR hiện tại vào group riêng của người dùng để gửi cảnh báo đúng đối tượng.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier
                     ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }
}
