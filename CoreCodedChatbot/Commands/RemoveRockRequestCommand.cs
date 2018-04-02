using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "removerequest", "rrr", "removerockrequest", "removesong", "rs", "removerequest" }, false)]
    public class RemoveRockRequestCommand : ICommand
    {
        public RemoveRockRequestCommand()
        { }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var success = PlaylistHelper.RemoveRockRequests(username, commandText, isMod);

            client.SendMessage(success
                ? $"Hi @{username}, I have removed number: {commandText} from the queue."
                : $"Hi @{username}, I can't do that. Please use the command as follows: !removerequest <playlist index>. If your usage is correct you may not own the request or the playlist order has recently changed");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(
                $"Hey @{username}, this command is used to remove one of your requests from the queue. Use !mrr to check your SongRequestIndex. Usage: !removerequest <SongRequestIndex>");
        }
    }
}
