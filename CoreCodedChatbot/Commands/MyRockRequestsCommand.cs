using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "myrequests", "mrr", "myrockrequests", "mysongs", "myrequest", "mysong", "pos", "position" }, false)]
    public class MyRockRequestsCommand : ICommand
    {
        private readonly ConfigModel config;
        private HttpClient playlistClient;

        public MyRockRequestsCommand(ConfigModel config)
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
            var request = await playlistClient.PostAsync("GetUserRequests", HttpClientHelper.GetJsonData(username));

            if (request.IsSuccessStatusCode)
            {
                var requests =
                    JsonConvert.DeserializeObject<GetUserRequestsResponse>(await request.Content.ReadAsStringAsync());

                client.SendMessage(config.StreamerChannel,
                    $"Hey @{username}, you have requested: {requests.UserRequests}");
                return;
            }

            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, I couldn't check your requests at the moment. Please try again in a sec");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command tells you which songs you have currently requested.");
        }
    }
}
