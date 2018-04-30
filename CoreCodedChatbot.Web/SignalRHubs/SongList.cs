using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace CoreCodedChatbot.Web.SignalRHubs
{
    public class SongList : Hub
    {
        public async Task Send(object[] songText)
        {
            await this.Clients.All.InvokeAsync("Send", new[] { songText });
        }

        public async Task SendAll(object[] songText)
        {
            await this.Clients.All.InvokeAsync("SendAll", new[] {songText});
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Client {0} Connected", Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
