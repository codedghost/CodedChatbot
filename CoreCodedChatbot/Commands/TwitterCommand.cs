using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "twitter" }, false)]
    public class TwitterCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var config = ConfigHelper.GetConfig();
            client.SendMessage($"Follow me on twitter too: {config.TwitterLink}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command outputs the twitter link from time to time.");
        }
    }
}
