using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "customsforge", "cf", "customforge" }, false)]
    public class CustomsforgeCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage($"Create a login and find songs to choose from at http://ignition.customsforge.com");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command outputs where to find CDLC from time to time.");
        }
    }
}
