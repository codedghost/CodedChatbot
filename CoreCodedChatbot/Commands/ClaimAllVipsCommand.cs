using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "claimallvips", "claimall"}, false)]
    public class ClaimAllVipsCommand : ICommand
    {
        private BytesHelper bytesHelper;
        private ConfigModel config;

        public ClaimAllVipsCommand(BytesHelper bytesHelper, ConfigModel config)
        {
            this.config = config;
            this.bytesHelper = bytesHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var giveTokenSuccess = bytesHelper.ConvertAllBytes(username);

            client.SendMessage(joinedChannel, giveTokenSuccess ? $"Hey @{username}, I've converted your Byte(s) to VIP(s) :D" : $"Hey @{username}, it looks like you don't have enough byte(s) to do that. Stick around and you'll have enough in no time!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, "This command will convert all of your bytes to VIP tokens!");
        }
    }
}
