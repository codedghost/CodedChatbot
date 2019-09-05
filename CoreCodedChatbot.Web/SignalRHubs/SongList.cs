using System;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace CoreCodedChatbot.Web.SignalRHubs
{
    public class SongList : Hub
    {
        private ConfigModel config { get; set; }

        public SongList(IConfigService configService)
        {
            this.config = configService.GetConfig();
        }

        public async Task SendAll(SongListHubModel data)
        {
            var psk = config.SignalRKey;

            if (psk == data.psk)
            {
                var currentSong = data.currentSong;
                var regularRequests = data.regularRequests;
                var vipRequests = data.vipRequests;
                await this.Clients.All.SendCoreAsync("SendAll", new object[] { currentSong, regularRequests, vipRequests });
            }
        }

        public async Task UpdateById(SongListSingleSongModel updateModel)
        {
            var psk = config.SignalRKey;

            if (psk == updateModel.psk)
            {
                var updateSong = updateModel.PlaylistItem;

                await Clients.All.SendCoreAsync("UpdateSong", new object[] { updateSong });
            }
        }
        
        public async Task RemoveById(SongListSingleSongModel removeModel)
        {
            var psk = config.SignalRKey;

            if (psk == removeModel.psk)
            {
                var removeSong = removeModel.PlaylistItem.songRequestId;

                await Clients.All.SendCoreAsync("RemoveSong", new object[] { removeSong });
            }
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Client {0} Connected", Context.ConnectionId);
            return base.OnConnectedAsync();
        }
    }
}
