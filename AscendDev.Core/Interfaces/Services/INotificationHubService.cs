namespace AscendDev.Core.Interfaces.Services;

public interface INotificationHubService
{
    Task SendNotificationToUserAsync(Guid userId, object notification);
}