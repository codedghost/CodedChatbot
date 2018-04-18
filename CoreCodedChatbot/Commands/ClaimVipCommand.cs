using System;
using System.Collections.Generic;
using System.Text;

using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.CustomAttributes;

using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "claimvip", "convertvip" }, false)]
    public class ClaimVipCommand : ICommand
    {
        private readonly BytesHelper bytesHelper;

        public ClaimVipCommand(BytesHelper bytesHelper)
        {
            this.bytesHelper = bytesHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var giveTokenSuccess = int.TryParse(commandText, out var numberOfTokens) 
                ? bytesHelper.ConvertByte(username, numberOfTokens) 
                : bytesHelper.ConvertByte(username);

            client.SendMessage(giveTokenSuccess ? $"Hey @{username}, I've converted your Byte(s) to VIP(s) :D" : $"Hey @{username}, it looks like you don't have a full Byte yet. Stick around and you'll have one in no time!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command will convert Bytes to VIPs if you have enough!");
        }
    }
}
