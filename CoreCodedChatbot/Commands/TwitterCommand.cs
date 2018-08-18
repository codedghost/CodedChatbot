using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "twitter" }, false)]
    public class TwitterCommand : ICommand
    {
        private readonly ConfigModel config;

        public TwitterCommand(IConfigHelper configHelper)
        {
            this.config = configHelper.GetConfig();
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, $"Follow me on twitter too: {config.TwitterLink}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs the twitter link from time to time.");
        }
    }
}
