using System;
using System.Threading.Tasks;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CoreCodedChatbot.Web.SignalRHubs
{
    public class SongList : Hub
    {
        private ConfigModel config { get; set; }

        public SongList(IConfigHelper configHelper)
        {
            this.config = configHelper.GetConfig();
        }

        public async Task SendAll(SongListHubModel data)
        {
            var psk = config.SignalRKey;

            if (psk == data.psk)
            {
                var dataObj = data.requests;
                await this.Clients.All.SendCoreAsync("SendAll", new object[] { dataObj });
            }
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Client {0} Connected", Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
