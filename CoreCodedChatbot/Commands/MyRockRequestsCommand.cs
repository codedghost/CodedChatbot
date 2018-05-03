using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "myrequests", "mrr", "myrockrequests", "mysongs", "myrequest", "mysong", "pos", "position" }, false)]
    public class MyRockRequestsCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;
        private readonly ConfigModel config;

        public MyRockRequestsCommand(PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.playlistHelper = playlistHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var requests = playlistHelper.GetUserRequests(username);

            client.SendMessage(config.StreamerChannel, $"Hey @{username}, you have requested: {requests}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command tells you which songs you have currently requested.");
        }
    }
}
