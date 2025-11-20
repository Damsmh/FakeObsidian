using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace FakeObsidian.Api.Hubs
{
    [Authorize]
    [SignalRHub]
    public class ChatHub : Hub
    {
        [SignalRMethod("SendMessage")]
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
