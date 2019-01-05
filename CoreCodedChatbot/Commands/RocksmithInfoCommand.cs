using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"rsinfo"}, false)]
    public class RocksmithInfoCommand : ICommand
    {
        private readonly CommandHelper commandHelper;
        private readonly ConfigModel config;

        public RocksmithInfoCommand(CommandHelper commandHelper, ConfigModel config)
        {
            this.commandHelper = commandHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            // Run all Rocksmith info commands at once
            commandHelper.ProcessCommand("rsinfo", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("list", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("howtorequest", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("cf", client, "Chatbot", string.Empty, true, joinedChannel);
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs general stream info from time to time.");
        }
    }
}
