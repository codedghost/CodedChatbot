using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "howtorequest", "helprequest" }, false)]
    public class HowToRequestCommand : ICommand
    {
        private readonly ConfigModel config;
        private readonly HttpClient playlistClient;

        public HowToRequestCommand(ConfigModel config)
        {
            this.config = config;
            this.playlistClient = new HttpClient
            {
                BaseAddress = new Uri(config.PlaylistApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var request = await playlistClient.GetAsync("IsPlaylistOpen");
            if (!request.IsSuccessStatusCode) return;

            var isOpen = JsonConvert.DeserializeObject<bool>(await request.Content.ReadAsStringAsync());

            if (isOpen)
            {
                client.SendMessage(joinedChannel,
                    $"To request a song just use: !request <SongArtist> - <SongTitle> - (Guitar or Bass)");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs how to request a song from time to time.");
        }
    }
}
