using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "removesupervip", "removesuper", "rmsvip", "deletesuper", "deletesupervip", "delsvip" }, false)]
    public class RemoveSuperVipCommand : ICommand
    {
        private readonly ConfigModel config;
        private HttpClient playlistClient;

        public RemoveSuperVipCommand(ConfigModel config)
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
            var result = await playlistClient.PostAsync("RemoveSuperVipCommand", HttpClientHelper.GetJsonData(new { username }));

            client.SendMessage(joinedChannel,
                result.IsSuccessStatusCode
                    ? $"Hey @{username}, your SuperVIP request has been removed and refunded"
                    : $"Hey @{username}, I couldn't remove your request, please try again in a sec.");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will let you remove your Super VIP request! Usage: !removesuper");
        }
    }
}
