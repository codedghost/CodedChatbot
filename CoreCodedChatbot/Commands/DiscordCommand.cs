using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "discord" }, false)]
    public class DiscordCommand : ICommand
    {
        private readonly IConfigService _configService;

        public DiscordCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            // load discord link from config
            client.SendMessage(joinedChannel, $"Join us on discord: { _configService.Get<string>("DiscordLink") }");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs the discord link from time to time.");
        }
    }
}
