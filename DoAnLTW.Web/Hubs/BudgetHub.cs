using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DoAnLTW.Web.Hubs;

[Authorize]
public class BudgetHub : Hub
{
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
