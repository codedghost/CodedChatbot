using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "removesupervip", "removesuper", "rmsvip", "deletesuper", "deletesupervip", "delsvip" }, false)]
    public class RemoveSuperVipCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public RemoveSuperVipCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }
        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var success = await _playlistApiClient.RemoveSuperVip(new RemoveSuperVipRequest
            {
                Username = username
            });

            client.SendMessage(joinedChannel,
                success
                    ? $"Hey @{username}, your SuperVIP request has been removed and refunded"
                    : $"Hey @{username}, I couldn't remove your request, please try again in a sec.");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will let you remove your Super VIP request! Usage: !removesuper");
        }
    }
}
