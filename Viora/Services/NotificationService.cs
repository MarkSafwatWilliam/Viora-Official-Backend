using Microsoft.AspNetCore.SignalR;
using Viora.Hubs;

namespace Viora.Services
{
    public class NotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public NotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NewChatAdded(int userId, int chatId, string title)
        {
            await _hubContext.Clients.Group(userId.ToString())
                .SendAsync("NewChatAdded", new
                {
                    id = chatId,
                    title,
                    createdAt = DateTime.UtcNow
                });
        }
    }
}
