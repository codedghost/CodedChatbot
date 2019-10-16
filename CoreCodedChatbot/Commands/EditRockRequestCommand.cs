﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
    [CustomAttributes.ChatCommand(new[] { "editrequest", "err", "editrockrequest", "editsong", "edit" }, false)]
    public class EditRockRequestCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public EditRockRequestCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var result = _playlistApiClient.EditRequest(new EditSongRequest
            {
                username = username,
                commandText = commandText,
                isMod = isMod
            });

            client.SendMessage(joinedChannel,
                result != null
                    ? $"Hey @{username}, I have successfully changed your request to: {result.SongRequestText}"
                    : $"Hey @{username}, if you want to edit a regular request just use !edit <NewSongRequest>, otherwise include the VIP number like this: !edit <SongNumber> <NewSongRequest>");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, use this command to edit your request. Use !myrequests to check your SongRequestIndex. If you want to edit a regular request just use !edit <NewSongRequest>, otherwise include the VIP number like this: !edit <SongNumber> <NewSongRequest>");
        }
    }
}
