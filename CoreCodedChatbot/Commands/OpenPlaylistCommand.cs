using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "op", "openplaylist" }, true)]
    public class OpenPlaylistCommand : ICommand
    {
        private readonly ConfigModel config;
        private HttpClient playlistClient;

        public OpenPlaylistCommand(ConfigModel config)
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

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var request = await playlistClient.GetAsync("OpenPlaylist");

            client.SendMessage(config.StreamerChannel, request.IsSuccessStatusCode
                ? $"Hey @{username}, I have opened the playlist for you" 
                : $"Hey @{username}, I can't seem to open the playlist :(");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will open the playlist!");
        }
    }
}
