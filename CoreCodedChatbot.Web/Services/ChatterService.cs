using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.View;
using CoreCodedChatbot.Web.Interfaces;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Web.Services
{
    public class ChatterService : IChatterService
    {
        private ChatViewersModel Chatters { get; set; }

        private Timer chatterTimer { get; set; }

        private ConfigModel config { get; set; }

        public ChatterService(IConfigHelper configHelper)
        {
            config = configHelper.GetConfig();

            chatterTimer = new Timer((x) => { UpdateChatters(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async void UpdateChatters()
        {
            var httpClient = new HttpClient();
            var currentChattersJson = await httpClient.GetStringAsync($"https://tmi.twitch.tv/group/user/{config.StreamerChannel}/chatters");
            // process json into username list.
            Chatters = JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson);
        }

        public ChatViewersModel GetCurrentChatters()
        {
            return Chatters;
        }
    }
}
