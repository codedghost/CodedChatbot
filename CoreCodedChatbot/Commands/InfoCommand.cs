﻿using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"info"}, false)]
    public class InfoCommand : ICommand
    {
        private readonly CommandHelper commandHelper;
        private readonly ConfigModel config;

        public InfoCommand(CommandHelper commandHelper, ConfigModel config)
        {
            this.commandHelper = commandHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            // Run all stream info commands at once
            commandHelper.ProcessCommand("discord", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("twitter", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("yt", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("merch", client, "Chatbot", string.Empty, true, joinedChannel);
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs general stream info from time to time.");
        }
    }
}
