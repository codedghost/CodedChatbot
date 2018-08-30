﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using Newtonsoft.Json;
using TwitchLib.Client;


namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "request", "rr", "sr", "songrequest", "rockrequest", "song" }, false)]
    public class RockRequestCommand : ICommand
    {
        private HttpClient playlistClient;
        private readonly ConfigModel config;

        public RockRequestCommand(ConfigModel config)
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
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(config.StreamerChannel, $"Hi @{username}, looks like you haven't included a request there!");
                return;
            }

            var request = await playlistClient.PostAsync("AddRequest", HttpClientHelper.GetJsonData(new {username, commandText}));
            var result = JsonConvert.DeserializeObject<AddRequestResponse>(await request.Content.ReadAsStringAsync());

            if (result.Result == AddRequestResult.PlaylistClosed)
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, the playlist is currently closed. If you want to request a song still, try !vip");
            }
            else if (result.Result == AddRequestResult.NoMultipleRequests)
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, you can only have one non-vip request in the list!");
            }
            else
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, I have queued {commandText} for you!");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, this command is used to add a song request to the queue. Usage: !request <SongArtist> - <SongName>");
        }
    }
}
