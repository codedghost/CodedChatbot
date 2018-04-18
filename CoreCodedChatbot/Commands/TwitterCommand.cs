using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "twitter" }, false)]
    public class TwitterCommand : ICommand
    {
        private readonly ConfigModel config = ConfigHelper.GetConfig();

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var config = ConfigHelper.GetConfig();
            client.SendMessage(config.StreamerChannel, $"Follow me on twitter too: {config.TwitterLink}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs the twitter link from time to time.");
        }
    }
}
