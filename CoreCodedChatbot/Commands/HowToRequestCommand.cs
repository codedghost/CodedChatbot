using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "howtorequest", "helprequest" }, false)]
    public class HowToRequestCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public HowToRequestCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var request = await _playlistApiClient.IsPlaylistOpen();

            if (request == PlaylistState.Open)
            {
                client.SendMessage(joinedChannel,
                    $"To request a song just use: !request <SongArtist> - <SongTitle> - (Guitar or Bass)");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs how to request a song from time to time.");
        }
    }
}
