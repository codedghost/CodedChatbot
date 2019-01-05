﻿using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"social", "socials"}, false)]
    public class SocialCommand : ICommand
    {
        private ConfigModel config;
        private CommandHelper commandHelper;

        public SocialCommand(ConfigModel config, CommandHelper commandHelper)
        {
            this.config = config;
            this.commandHelper = commandHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            commandHelper.ProcessCommand("discord", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("yt", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("insta", client, "Chatbot", string.Empty, true, joinedChannel);
            commandHelper.ProcessCommand("twitter", client, "Chatbot", string.Empty, true, joinedChannel);
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will output all of {config.StreamerChannel}'s social media links");
        }
    }
}
