using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "removesupervip", "removesuper", "rmsvip", "deletesuper", "deletesupervip", "delsvip" }, false)]
    public class RemoveSuperVipCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public RemoveSuperVipCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }
        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var success = _playlistApiClient.RemoveSuperVip(new RemoveSuperVipRequest
            {
                username = username
            });

            client.SendMessage(joinedChannel,
                success
                    ? $"Hey @{username}, your SuperVIP request has been removed and refunded"
                    : $"Hey @{username}, I couldn't remove your request, please try again in a sec.");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will let you remove your Super VIP request! Usage: !removesuper");
        }
    }
}
