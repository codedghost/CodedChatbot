using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "op", "openplaylist" }, true)]
    public class OpenPlaylistCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public OpenPlaylistCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var success = await _playlistApiClient.OpenPlaylist();

            client.SendMessage(joinedChannel, success
                ? $"Hey @{username}, I have opened the playlist for you" 
                : $"Hey @{username}, I can't seem to open the playlist :(");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will open the playlist!");
        }
    }
}
