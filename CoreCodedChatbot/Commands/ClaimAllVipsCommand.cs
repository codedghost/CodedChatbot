using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "claimallvips", "claimall"}, false)]
    public class ClaimAllVipsCommand : ICommand
    {
        private BytesHelper bytesHelper;
        private ConfigModel config;

        public ClaimAllVipsCommand(BytesHelper bytesHelper, ConfigModel config)
        {
            this.config = config;
            this.bytesHelper = bytesHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var giveTokenSuccess = bytesHelper.ConvertAllBytes(username);

            client.SendMessage(config.StreamerChannel, giveTokenSuccess ? $"Hey @{username}, I've converted your Byte(s) to VIP(s) :D" : $"Hey @{username}, it looks like you don't have enough byte(s) to do that. Stick around and you'll have enough in no time!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, "This command will convert all of your bytes to VIP tokens!");
        }
    }
}
