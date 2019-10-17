using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new [] { "promote"}, false)]
    public class PromoteCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;
        private readonly IVipHelper _vipHelper;

        public PromoteCommand(IPlaylistApiClient playlistApiClient, IVipHelper vipHelper)
        {
            _playlistApiClient = playlistApiClient;
            _vipHelper = vipHelper;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (_vipHelper.CanUseVipRequest(username))
            {
                var playlistPosition = await _playlistApiClient.PromoteSong(new PromoteSongRequest
                {
                    username = username
                });

                // TODO: This really should be an object returned
                if (playlistPosition != -1)
                {
                    client.SendMessage(joinedChannel, playlistPosition == -1
                        ? $"Hey @{username}, I can't find a song at that position! Please check your requests with !myrequests"
                        : playlistPosition == -2
                            ? $"Hey @{username}, I'm sorry but that request doesn't seem to belong to you. Please check your requests with !myrequests"
                            : playlistPosition == 0
                                ? $"Hey @{username}, something seems to have gone wrong. Please try again in a minute or two"
                                : $"Hey @{username}, I have promoted your request to #{playlistPosition} for you!");


                    if (playlistPosition > 0) _vipHelper.UseVipRequest(username);
                    return;
                }

                client.SendMessage(joinedChannel,
                    $"Hey @{username}, sorry I can't promote your request right now, please try again in a sec");
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
                $"Hey @{username}, if you have a VIP token, this command will bump your song request right to the top of the queue. Usage: !promote");
        }
    }
}
