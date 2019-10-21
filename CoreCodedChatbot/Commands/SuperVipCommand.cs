using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"supervip", "svip", "super"}, false)]
    public class SuperVipCommand : ICommand
    {
        private IVipHelper _vipHelper;
        private readonly IConfigService _configService;
        private readonly IPlaylistApiClient _playlistApiClient;

        public SuperVipCommand(IVipHelper vipHelper, IConfigService configService, IPlaylistApiClient playlistApiClient)
        {
            _vipHelper = vipHelper;
            _configService = configService;
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            // Check there is a request given
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, I'm sorry but it looks like you haven't included a request! !svip <artistname> - <songname> - (guitar or bass)");
                return;
            }

            // Check the user has enough VIPs
            if (_vipHelper.CanUseSuperVipRequest(username))
            {
                var addSuperResponse = await _playlistApiClient.AddSuperVip(new AddSuperVipRequest
                {
                    username = username,
                    commandText = commandText
                });

                if (addSuperResponse != null)
                {
                    switch (addSuperResponse.Result)
                    {
                        case AddRequestResult.PlaylistVeryClosed:
                            client.SendMessage(joinedChannel,
                                $"Hey @{username}, the playlist is currently very closed. No Requests allowed.");
                            return;
                        case AddRequestResult.OnlyOneSuper:
                            client.SendMessage(joinedChannel,
                                $"Hey @{username}, sorry but there can only be one SuperVIP in the queue at a time!");
                            return;
                    }

                    _vipHelper.UseSuperVipRequest(username);
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, I have queued {commandText} for you, your request will be played next!");

                    return;
                }
            }
            else
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, it looks like you don't have enough VIP tokens :( You need at least {_configService.Get<string>("SuperVipCost")} tokens to request a SuperVIP");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command adds a SuperVIP to the queue for you! This request lets you completely jump the queue, but it comes with a high price!");
        }
    }
}
