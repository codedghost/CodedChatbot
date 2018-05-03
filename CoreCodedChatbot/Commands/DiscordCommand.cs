using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "discord" }, false)]
    public class DiscordCommand : ICommand
    {
        private readonly ConfigModel config;

        public DiscordCommand(IConfigHelper configHelper)
        {
            this.config = configHelper.GetConfig();
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            // load discord link from config
            client.SendMessage(config.StreamerChannel, $"Join us on discord: { config.DiscordLink }");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs the discord link from time to time.");
        }
    }
}
