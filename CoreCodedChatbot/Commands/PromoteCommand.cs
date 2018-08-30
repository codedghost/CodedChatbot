﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new [] { "promote"}, false)]
    public class PromoteCommand : ICommand
    {
        private VipHelper vipHelper;
        private HttpClient playlistClient;
        private ConfigModel config;

        public PromoteCommand(ConfigModel config, VipHelper vipHelper)
        {
            this.vipHelper = vipHelper;
            this.config = config;

            this.playlistClient = new HttpClient
            {
                BaseAddress = new Uri(config.PlaylistApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            if (vipHelper.CanUseVipRequest(username))
            {
                var request = await playlistClient.PostAsync("PromoteRequest",
                    HttpClientHelper.GetJsonData(new {username}));
                var playlistPosition = JsonConvert.DeserializeObject<int>(await request.Content.ReadAsStringAsync());
                client.SendMessage(config.StreamerChannel, playlistPosition == -1
                    ? $"Hey @{username}, I can't find a song at that position! Please check your requests with !myrequests"
                    : playlistPosition == -2
                        ? $"Hey @{username}, I'm sorry but that request doesn't seem to belong to you. Please check your requests with !myrequests"
                        : playlistPosition == 0
                            ? $"Hey @{username}, something seems to have gone wrong. Please try again in a minute or two"
                            : $"Hey @{username}, I have promoted your request to #{playlistPosition} for you!");

                if (playlistPosition > 0) vipHelper.UseVipRequest(username);
                return;
            }
            else
            {
                client.SendMessage(config.StreamerChannel,
                    $"Hey @{username}, it looks like you don't have any remaining VIP requests. Please use the standard !request command.");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, if you have a VIP token, this command will bump your song request right to the top of the queue. Usage: !promote");
        }
    }
}