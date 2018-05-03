using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "customsforge", "cf", "customforge" }, false)]
    public class CustomsforgeCommand : ICommand
    {
        private readonly ConfigModel config;

        public CustomsforgeCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, $"Create a login and find songs to choose from at http://ignition.customsforge.com");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs where to find CDLC from time to time.");
        }
    }
}
