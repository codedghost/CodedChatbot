using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "editsupervip", "editsuper", "esvip"}, false)]
    public class EditSuperVipCommand : ICommand
    {
        private readonly ConfigModel config;
        private HttpClient playlistClient;

        public EditSuperVipCommand(ConfigModel config)
        {
            this.playlistClient = new HttpClient
            {
                BaseAddress = new Uri(config.PlaylistApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
            this.config = config;
        }
        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var result = await playlistClient.PostAsync("EditSuperVipRequest", HttpClientHelper.GetJsonData(new { username, commandText }));
            var response = JsonConvert.DeserializeObject<EditRequestResponse>(await result.Content.ReadAsStringAsync());

            client.SendMessage(joinedChannel,
                result.IsSuccessStatusCode
                    ? response.SyntaxError ? $"Hey @{username}, I couldn't change your request, please check you have a Super VIP in the queue and it is not about to be played" :
                        $"Hey @{username}, I have successfully changed your request to: {response.SongRequestText}"
                    : $"Hey @{username}, I couldn't request your change, please try again in a sec.");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will let you edit your Super VIP request! Usage: !editsuper <songartist> - <songname> (guitar or bass)");
        }
    }
}
