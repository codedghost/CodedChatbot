using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "myrequests", "mrr", "myrockrequests", "mysongs" }, false)]
    public class MyRockRequestsCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        public MyRockRequestsCommand(PlaylistHelper playlistHelper)
        {
            this.playlistHelper = playlistHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var requests = playlistHelper.GetUserRequests(username);

            client.SendMessage($"Hey @{username}, you have requested: {requests}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command tells you which songs you have currently requested.");
        }
    }
}
