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
    [ChatCommand(new []{"clearrequests", "clearrockrequests" }, true)]
    public class ClearRockRequestsCommand : ICommand
    {
        private readonly ConfigModel config;
        private HttpClient playlistClient;

        public ClearRockRequestsCommand(ConfigModel config)
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
            var response = await playlistClient.GetAsync("ClearRockRequests");

            client.SendMessage(config.StreamerChannel,
                response.IsSuccessStatusCode
                    ? $"Hey @{username}, I've cleared all requests for you!"
                    : $"Hey @{username} I couldn't clear the requests! Something went wrong :(");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will clear all requests from the queue.");
        }
    }
}
