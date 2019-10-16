using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "removerequest", "rrr", "removerockrequest", "removesong", "removerequest" }, false)]
    public class RemoveRockRequestCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public RemoveRockRequestCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var success = _playlistApiClient.RemoveRockRequests(new RemoveSongRequest
            {
                username = username,
                commandText = commandText,
                isMod = isMod
            });

            client.SendMessage(joinedChannel, success
                ? $"Hi @{username}, I have removed number: {commandText} from the queue."
                : $"Hi @{username}, I can't do that. If you are trying to remove a regular request just use !removerequest, if it's a VIP then include the playlist index: !removerequest <SongRequestIndex>");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command is used to remove one of your requests from the queue.  If you are trying to remove a regular request just use !removerequest, if it's a VIP then include the playlist index: !removerequest <SongRequestIndex>");
        }
    }
}
