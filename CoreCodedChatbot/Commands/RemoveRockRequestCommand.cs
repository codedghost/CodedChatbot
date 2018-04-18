using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "removerequest", "rrr", "removerockrequest", "removesong", "rs", "removerequest" }, false)]
    public class RemoveRockRequestCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly ConfigModel config = ConfigHelper.GetConfig();

        public RemoveRockRequestCommand(PlaylistHelper playlistHelper)
        {
            this.playlistHelper = playlistHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var success = playlistHelper.RemoveRockRequests(username, commandText, isMod);

            client.SendMessage(config.StreamerChannel, success
                ? $"Hi @{username}, I have removed number: {commandText} from the queue."
                : $"Hi @{username}, I can't do that. Please use the command as follows: !removerequest <playlist index>. If your usage is correct you may not own the request or the playlist order has recently changed");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, this command is used to remove one of your requests from the queue. Use !mrr to check your SongRequestIndex. Usage: !removerequest <SongRequestIndex>");
        }
    }
}
