using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "merch" }, false)]
    public class MerchCommand : ICommand
    {
        private ConfigModel config;

        public MerchCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel,
                username == "Chatbot"
                    ? $"Check out {config.StreamerChannel}'s merch over at: {config.MerchLink}"
                    : $"Hey @{username}, check out {config.StreamerChannel}'s merch over at: {config.MerchLink}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, this command will output a link to {config.StreamerChannel}'s merch page from time to time");
        }
    }
}
