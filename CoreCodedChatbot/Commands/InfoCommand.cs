using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{"info"}, false)]
    public class InfoCommand : ICommand
    {
        private readonly CommandHelper commandHelper;
        private readonly ConfigModel config;

        public InfoCommand(CommandHelper commandHelper, ConfigModel config)
        {
            this.commandHelper = commandHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            // Run all four info commands at once
            commandHelper.ProcessCommand("howtorequest", client, "Chatbot", string.Empty, true);
            commandHelper.ProcessCommand("customsforge", client, "Chatbot", string.Empty, true);
            commandHelper.ProcessCommand("discord", client, "Chatbot", string.Empty, true);
            commandHelper.ProcessCommand("twitter", client, "Chatbot", string.Empty, true);
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs general stream info from time to time.");
        }
    }
}
