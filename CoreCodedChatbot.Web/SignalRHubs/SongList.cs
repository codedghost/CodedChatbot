using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Models.Data;
using CoreCodedChatbot.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CoreCodedChatbot.Web.SignalRHubs
{
    public class SongList : Hub
    {
        private ConfigModel config { get; set; }

        public SongList(ConfigModel config)
        {
            this.config = config;
        }

        public async Task Send(SongListHubModel data)
        {
            var psk = config.SignalRKey;

            if (psk == data.psk)
            {
                await this.Clients.All.InvokeAsync("Send", new[] { data.requests });
            }
        }

        public async Task SendAll(SongListHubModel data)
        {
            var psk = config.SignalRKey;

            if (psk == data.psk)
            {
                await this.Clients.All.InvokeAsync("SendAll", new[] {data.requests});
            }
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Client {0} Connected", Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
