using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.SignalRHubs;
using Microsoft.AspNetCore.SignalR;

namespace CoreCodedChatbot.Web.Services
{
    public class SignalRHeartbeatService : ISignalRHeartbeatService
    {
        private IHubContext<SongList> hubContext;

        private Timer signalRHeartbeat;

        public SignalRHeartbeatService(IHubContext<SongList> hubContext)
        {
            this.hubContext = hubContext;

            signalRHeartbeat =
                new Timer(
                    (x) => { OnTimerCallback(null); }, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
        }

        public void OnTimerCallback(object state)
        {
            NotifyClients();
        }

        public async void NotifyClients()
        {
            await hubContext.Clients.All.SendCoreAsync("Heartbeat", new object[]{});
        }
    }
}
