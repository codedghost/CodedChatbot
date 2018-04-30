using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "discord" }, false)]
    public class DiscordCommand : ICommand
    {
        private readonly IConfigHelper configHelper;

        public DiscordCommand(IConfigHelper configHelper)
        {
            this.configHelper = configHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            // load discord link from config
            var config = configHelper.GetConfig();
            client.SendMessage(config.StreamerChannel, $"Join us on discord: { config.DiscordLink }");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            var config = configHelper.GetConfig();
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs the discord link from time to time.");
        }
    }
}
