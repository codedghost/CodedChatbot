using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "cp", "closeplaylist", "sp", "shutplaylist" }, true)]
    public class ClosePlaylistCommand : ICommand
    {
        private readonly HttpClient playlistClient;
        private readonly ConfigModel config;

        public ClosePlaylistCommand(ConfigModel config)
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
            HttpResponseMessage response;
            if (commandText.ToLower().Contains("very"))
            {
                response = await playlistClient.GetAsync("VeryClosePlaylist");
            } 
            else
            {
                response = await playlistClient.GetAsync("ClosePlaylist");
            }

            client.SendMessage(config.StreamerChannel, response.IsSuccessStatusCode
                ? $"Hey @{username}, I have closed the playlist{(commandText == "very" ? " completely" : string.Empty)}!" 
                : $"Hey {username}, I can't seem to close the playlist for some reason :(");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will close the playlist!");
        }
    }
}
