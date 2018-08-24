using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "followme"}, true)]
    public class FollowMeCommand : ICommand
    {
        private readonly ConfigModel config;

        public FollowMeCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, $"Hope you like the stream! Go on... Give that Follow button a quick click ;)");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command gently reminds everyone to follow the channel!");
        }
    }
}
