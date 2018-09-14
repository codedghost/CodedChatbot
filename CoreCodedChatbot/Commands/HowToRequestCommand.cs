using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "howtorequest", "helprequest" }, false)]
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

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var request = await playlistClient.GetAsync("IsPlaylistOpen");
            if (!request.IsSuccessStatusCode) return;

            var isOpen = JsonConvert.DeserializeObject<bool>(await request.Content.ReadAsStringAsync());

            if (isOpen || username != "Chatbot")
            {
                client.SendMessage(config.StreamerChannel,
                    $"To request a song just use: !request <SongArtist> - <SongTitle> - (Guitar or Bass)");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs how to request a song from time to time.");
        }
    }
}
