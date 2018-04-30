using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "followme"}, true)]
    public class FollowMeCommand : ICommand
    {
        private readonly ConfigModel config;

        public FollowMeCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, $"Hope you like what you're hearing! Give that Follow button a quick click ;)");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command gently reminds everyone to follow the channel!");
        }
    }
}
