using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;


namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "request", "rr", "sr", "songrequest", "rockrequest", "song" }, false)]
    public class RockRequestCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public RockRequestCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hi @{username}, looks like you haven't included a request there!");
                return;
            }

            var result = _playlistApiClient.AddSong(new AddSongRequest
            {
                username = username,
                commandText = commandText,
                isVipRequest = false
            });

            string message;
            if (result != null)
            {
                switch (result.Result)
                {
                    case AddRequestResult.PlaylistVeryClosed:
                        message = $"Hey @{username}, the playlist is currently very closed. No Requests allowed.";
                        break;
                    case AddRequestResult.PlaylistClosed:
                        message =
                            $"Hey @{username}, the playlist is currently closed. If you want to request a song still, try !vip";
                        break;
                    case AddRequestResult.NoMultipleRequests:
                        message = $"Hey @{username}, you can only have one non-vip request in the list!";
                        break;
                    default:
                        message = $"Hey @{username}, I have queued {commandText} for you!";
                        break;
                }
            }
            else
            {
                message = $"Hey @{username}, something's gone wrong. Please try again soon!";
            }

            client.SendMessage(joinedChannel, message);
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command is used to add a song request to the queue. Usage: !request <SongArtist> - <SongName>");
        }
    }
}
