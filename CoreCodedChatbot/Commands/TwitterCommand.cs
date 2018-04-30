using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "twitter" }, false)]
    public class TwitterCommand : ICommand
    {
        private readonly IConfigHelper configHelper;

        public TwitterCommand(IConfigHelper configHelper)
        {
            this.configHelper = configHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var config = configHelper.GetConfig();
            client.SendMessage(config.StreamerChannel, $"Follow me on twitter too: {config.TwitterLink}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            var config = configHelper.GetConfig();
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs the twitter link from time to time.");
        }
    }
}
