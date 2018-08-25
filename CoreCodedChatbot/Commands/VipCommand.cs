using System;
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
    [ChatCommand(new []{"vip", "viprequest"}, false)]
    public class VipCommand : ICommand
    {
        private HttpClient playlistClient;
        private readonly VipHelper vipHelper;

        private readonly ConfigModel config;

        public VipCommand(VipHelper vipHelper, ConfigModel config)
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
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, looks like you haven't included a request there!");
                return;
            }

            if (vipHelper.CanUseVipRequest(username))
            {
                var playlistPosition = 0;
                var songIndex = 0;
                if (int.TryParse(commandText.Trim('#'), out songIndex))
                {
                    var request = await playlistClient.PostAsync("PromoteRequest",
                        HttpClientHelper.GetJsonData(new {username, songIndex = songIndex - 1}));
                    playlistPosition = JsonConvert.DeserializeObject<int>(await request.Content.ReadAsStringAsync());
                    client.SendMessage(config.StreamerChannel, playlistPosition == -1
                        ? $"Hey @{username}, I can't find a song at that position! Please check your requests with !myrequests"
                        : playlistPosition == -2
                            ? $"Hey @{username}, I'm sorry but that request doesn't seem to belong to you. Please check your requests with !myrequests"
                            : playlistPosition == 0
                                ? $"Hey @{username}, something seems to have gone wrong. Please try again in a minute or two"
                                : $"Hey @{username}, I have promoted #{commandText} to #{playlistPosition} for you!");

                    if (playlistPosition > 0) vipHelper.UseVipRequest(username);
                    return;
                }

                var addRequest = await playlistClient.PostAsync("AddRequest",
                    HttpClientHelper.GetJsonData(new {username, commandText, isVipRequest = true}));

                var addResult =
                    JsonConvert.DeserializeObject<AddRequestResponse>(await addRequest.Content.ReadAsStringAsync());
                playlistPosition = addResult.PlaylistPosition;

                vipHelper.UseVipRequest(username);
                client.SendMessage(config.StreamerChannel,
                    $"Hey @{username}, I have queued {commandText} for you, you're #{playlistPosition} in the queue!");
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
                $"Hey @{username}, if you have a VIP request, this command will bump your song request right to the top of the queue. Usage: !vip <SongArtist> - <SongName> - (Guitar or Bass) OR !vip <SongNumber>");
        }
    }
}
