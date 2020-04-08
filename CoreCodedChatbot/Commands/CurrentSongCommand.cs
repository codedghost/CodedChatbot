using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new [] {"current", "now", "first"}, false)]
    public class CurrentSongCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public CurrentSongCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var currentSong = await _playlistApiClient.GetCurrentSongRequest();

            if (currentSong == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, sorry I couldn't find this info for you at the moment, please try again in a minute");
                return;
            }

            var songText = string.IsNullOrWhiteSpace(currentSong?.SongArtist)
                ? currentSong.SongName
                : $"{currentSong.SongArtist} - {currentSong.SongName} - {currentSong.InstrumentName}. Requested By {currentSong.RequesterUsername}";

            client.SendMessage(joinedChannel, $"Hey @{username}, The current song is: {songText}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will tell you the current song in the playlist!");
        }
    }
}