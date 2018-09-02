using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "removerequest", "rrr", "removerockrequest", "removesong", "rs", "removerequest" }, false)]
    public class RemoveRockRequestCommand : ICommand
    {
        private HttpClient playlistClient;
        private readonly ConfigModel config;

        public RemoveRockRequestCommand(ConfigModel config)
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
            var request = await playlistClient.PostAsync("RemoveRockRequests",
                HttpClientHelper.GetJsonData(new {username, commandText, isMod}));
            var success = request.IsSuccessStatusCode;

            client.SendMessage(config.StreamerChannel, success
                ? $"Hi @{username}, I have removed number: {commandText} from the queue."
                : $"Hi @{username}, I can't do that. If you are trying to remove a regular request just use !removerequest, if it's a VIP then include the playlist index: !removerequest <SongRequestIndex>");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, this command is used to remove one of your requests from the queue.  If you are trying to remove a regular request just use !removerequest, if it's a VIP then include the playlist index: !removerequest <SongRequestIndex>");
        }
    }
}
