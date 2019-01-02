using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "rcr", "removecurrentrequest" }, true)]
    public class RemoveCurrentRequestCommand : ICommand
    {
        private HttpClient playlistClient;
        private readonly ConfigModel config;

        public RemoveCurrentRequestCommand(ConfigModel config)
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
            var request = await playlistClient.GetAsync("ArchiveCurrentRequest");

            if (request.IsSuccessStatusCode)
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, the current request has been removed");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, I can't seem to do that right now, please try again in a sec");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will remove the current request from the queue.");
        }
    }
}
