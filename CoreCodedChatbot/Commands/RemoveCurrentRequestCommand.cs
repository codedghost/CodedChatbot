using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "rcr", "removecurrentrequest" }, true)]
    public class RemoveCurrentRequestCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public RemoveCurrentRequestCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var success = await _playlistApiClient.ArchiveCurrentRequest();

            if (success)
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
