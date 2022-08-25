using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"vip", "viprequest", "vsr"}, false)]
    public class VipCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public VipCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod,
            JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, looks like you haven't included a request there!");
                return;
            }

            var addSongResult = await _playlistApiClient.AddSong(new AddSongRequest
            {
                Username = username,
                CommandText = commandText,
                IsVipRequest = true
            });

            if (addSongResult != null)
            {
                switch (addSongResult.Result)
                {
                    case AddRequestResult.Success:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, I have queued {addSongResult.FormattedSongText} for you, you're #{addSongResult.PlaylistPosition} in the queue!");

                        return;
                    case AddRequestResult.PlaylistVeryClosed:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, the playlist is currently closed. No Requests allowed.");
                        return;
                    case AddRequestResult.NoRequestEntered:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, looks like you haven't included a request there!");
                        return;
                    case AddRequestResult.NotEnoughVips:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, sorry but you don't have a VIP token.");
                        return;
                    case AddRequestResult.UnSuccessful:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, I can't queue your VIP request right now, please try again in a sec");
                        return;
                }
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
                $"Hey @{username}, if you have a VIP request, this command will bump your song request right to the top of the queue. Usage (remove <>): !vip <SongArtist> - <SongName> - (Guitar or Bass)");
        }
    }
}
