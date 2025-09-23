using AscendDev.API.Hubs;
using AscendDev.Core.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace AscendDev.API.Services;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(Guid userId, object notification)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
    }
}