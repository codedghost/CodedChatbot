﻿using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{"info"}, false)]
    public class InfoCommand : ICommand
    {
        private readonly CommandHelper commandHelper;

        public InfoCommand(CommandHelper commandHelper)
        {
            this.commandHelper = commandHelper;
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
            client.SendMessage($"Hey @{username}, this command outputs general stream info from time to time.");
        }
    }
}
