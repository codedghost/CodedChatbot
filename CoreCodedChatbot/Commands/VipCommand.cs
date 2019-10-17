using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"vip", "viprequest"}, false)]
    public class VipCommand : ICommand
    {
        private readonly IVipHelper vipHelper;
        private readonly IPlaylistApiClient _playlistApiClient;

        public VipCommand(IVipHelper vipHelper, IPlaylistApiClient playlistApiClient)
        {
            this.vipHelper = vipHelper;
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, looks like you haven't included a request there!");
                return;
            }

            if (vipHelper.CanUseVipRequest(username))
            {
                var addSongResult = await _playlistApiClient.AddSong(new AddSongRequest
                {
                    username = username,
                    commandText = commandText,
                    isVipRequest = true
                });

                if (addSongResult != null)
                {
                    if (addSongResult.Result == AddRequestResult.PlaylistVeryClosed)
                    {
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, the playlist is currently very closed. No Requests allowed.");
                        return;
                    }

                    var playlistPosition = addSongResult.PlaylistPosition;

                    vipHelper.UseVipRequest(username);
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, I have queued {commandText} for you, you're #{playlistPosition} in the queue!");

                    return;
                }

                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I can't queue your VIP request right now, please try again in a sec");
            }
            else
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, it looks like you don't have any remaining VIP requests. Please use the standard !request command.");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, if you have a VIP request, this command will bump your song request right to the top of the queue. Usage: !vip <SongArtist> - <SongName> - (Guitar or Bass)");
        }
    }
}
