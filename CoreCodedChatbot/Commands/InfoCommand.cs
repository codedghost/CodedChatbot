﻿using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new [] { "chatbotinfocommand" }, true)]
    public class InfoCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, string.Format(commandText, string.IsNullOrEmpty(username) ? string.Empty : $"Hey {username}! "));
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this is a generic command that sends out stored command info");
        }
    }
}
