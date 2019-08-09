using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
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

        public ChatterService(IConfigService configService)
        {
            config = configService.GetConfig();

            chatterTimer = new Timer((x) => { UpdateChatters(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async void UpdateChatters()
        {
            var errorCounter = 0;

            try
            {
                errorCounter = 0;
                var httpClient = new HttpClient();
                var request =
                    await httpClient.GetAsync($"https://tmi.twitch.tv/group/user/{config.StreamerChannel}/chatters");

                if (!request.IsSuccessStatusCode) return;

                var currentChattersJson = await request.Content.ReadAsStringAsync();
                // process json into username list.
                Chatters = JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson);

                // Add broadcaster to modlist
                Chatters.chatters.moderators =
                    Chatters.chatters.moderators.Union(Chatters.chatters.broadcaster).ToArray();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"Could not access Twitch TMI resource. Exception:\n{e}\n{e.InnerException}");
                errorCounter++;

                if (errorCounter > 5)
                    Chatters.chatters.moderators = null;
            }
        }

        public ChatViewersModel GetCurrentChatters()
        {
            return Chatters;
        }
    }
}
