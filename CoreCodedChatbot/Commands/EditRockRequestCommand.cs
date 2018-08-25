using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "editrequest", "err", "editrockrequest", "editsong", "edit" }, false)]
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

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {    
            var result = playlistClient.PostAsync("EditRequest", HttpClientHelper.GetJsonData(new {username, commandText, isMod}));
            var response = JsonConvert.DeserializeObject<EditRequestResponse>(await result.Result.Content.ReadAsStringAsync());

            if (result.IsCompletedSuccessfully)
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username} I have successfully changed your request to: {response.SongRequestText}");
            }
            else
            {
                client.SendMessage(config.StreamerChannel,
                    response.SyntaxError
                        ? $"Hey @{username} command usage: !err <SongNumber> <NewSongRequest>"
                        : $"Hey @{username} it doesn't look like that's your request");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, use this command to edit your request. Use !myrequests to check your SongRequestIndex. Usage: !editrequest <Optional SongNumber> <NewSongRequest>");
        }
    }
}
