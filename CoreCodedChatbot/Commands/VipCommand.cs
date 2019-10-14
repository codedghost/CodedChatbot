using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
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
                var addRequest = await playlistClient.PostAsync("AddRequest",
                    HttpClientHelper.GetJsonData(new {username, commandText, isVipRequest = true}));

                if (addRequest.IsSuccessStatusCode)
                {
                    var textResult = await addRequest.Content.ReadAsStringAsync();
                    var addResult =
                        JsonConvert.DeserializeObject<AddRequestResponse>(textResult);
                    if (addResult.Result == AddRequestResult.PlaylistVeryClosed)
                    {
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, the playlist is currently very closed. No Requests allowed.");
                        return;
                    }

                    var playlistPosition = addResult.PlaylistPosition;

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
