using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;

namespace Viora.Hubs
{
    [Authorize]
    public class ChatHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId != null) {

                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }
    }
}
