using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "myrequests", "mrr", "myrockrequests", "mysongs", "myrequest", "mysong", "pos", "position" }, false)]
    public class MyRockRequestsCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public MyRockRequestsCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var requests = _playlistApiClient.GetUserRequests(username);

            if (requests != null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, you have requested: {requests.UserRequests}");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, I couldn't check your requests at the moment. Please try again in a sec");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command tells you which songs you have currently requested.");
        }
    }
}
