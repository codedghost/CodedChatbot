using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
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
        private readonly ConfigModel config;
        private HttpClient playlistClient;

        public EditRockRequestCommand(ConfigModel config)
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

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {    
            var result = await playlistClient.PostAsync("EditRequest", HttpClientHelper.GetJsonData(new {username, commandText, isMod}));
            var response = JsonConvert.DeserializeObject<EditRequestResponse>(await result.Content.ReadAsStringAsync());

            client.SendMessage(joinedChannel,
                result.IsSuccessStatusCode
                    ? $"Hey @{username}, I have successfully changed your request to: {response.SongRequestText}"
                    : $"Hey @{username}, if you want to edit a regular request just use !edit <NewSongRequest>, otherwise include the VIP number like this: !edit <SongNumber> <NewSongRequest>");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, use this command to edit your request. Use !myrequests to check your SongRequestIndex. If you want to edit a regular request just use !edit <NewSongRequest>, otherwise include the VIP number like this: !edit <SongNumber> <NewSongRequest>");
        }
    }
}
