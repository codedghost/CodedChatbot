using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
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

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hi @{username}, looks like you haven't included a request there!");
                return;
            }

            var result = await _playlistApiClient.AddSong(new AddSongRequest
            {
                Username = username,
                CommandText = commandText,
                IsVipRequest = false
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
                    case AddRequestResult.MaximumRegularRequests:
                        message = $"Hey @{username}, you've requested the max regular requests!";
                        break;
                    default:
                        message = $"Hey @{username}, I have queued {result.FormattedSongText} for you!";
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
                $"Hey @{username}, this command is used to add a song request to the queue. Usage (remove <>): !request <SongArtist> - <SongName>");
        }
    }
}
