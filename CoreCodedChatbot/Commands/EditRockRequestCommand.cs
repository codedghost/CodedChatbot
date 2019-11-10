using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "editrequest", "err", "editrockrequest", "editsong", "edit" }, false)]
    public class EditRockRequestCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public EditRockRequestCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var result = await _playlistApiClient.EditRequest(new EditSongRequest
            {
                Username = username,
                CommandText = commandText,
                IsMod = isMod
            });

            client.SendMessage(joinedChannel,
                result != null
                    ? $"Hey @{username}, I have successfully changed your request to: {result.SongRequestText}"
                    : $"Hey @{username}, if you want to edit a regular request just use !edit <NewSongRequest>, otherwise include the VIP number like this: !edit <SongNumber> <NewSongRequest>");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, use this command to edit your request. Use !myrequests to check your SongRequestIndex. If you want to edit a regular request just use !edit <NewSongRequest>, otherwise include the VIP number like this: !edit <SongNumber> <NewSongRequest>");
        }
    }
}
