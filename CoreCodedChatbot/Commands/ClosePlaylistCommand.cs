using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;
using Microsoft.AspNetCore.Localization;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "cp", "closeplaylist", "sp", "shutplaylist" }, true)]
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

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            HttpResponseMessage response;
            if (commandText.Equals("very", StringComparison.OrdinalIgnoreCase))
            {
                response = await playlistClient.GetAsync("VeryClosePlaylist");
            } 
            else
            {
                response = await playlistClient.GetAsync("ClosePlaylist");
            }

            client.SendMessage(joinedChannel, response.IsSuccessStatusCode
                ? $"Hey @{username}, I have closed the playlist{(commandText.Equals("very", StringComparison.OrdinalIgnoreCase) ? " completely" : string.Empty)}!"
                : $"Hey {username}, I can't seem to close the playlist for some reason :(");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will close the playlist!");
        }
    }
}
