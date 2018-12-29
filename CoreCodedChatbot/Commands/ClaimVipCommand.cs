using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "claimvip", "convertvip" }, false)]
    public class ClaimVipCommand : ICommand
    {
        private readonly BytesHelper bytesHelper;

        private readonly ConfigModel config;

        public ClaimVipCommand(BytesHelper bytesHelper, ConfigModel config)
        {
            this.bytesHelper = bytesHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var giveTokenSuccess = int.TryParse(commandText, out var numberOfTokens) 
                ? bytesHelper.ConvertByte(username, numberOfTokens) 
                : bytesHelper.ConvertByte(username);

            client.SendMessage(joinedChannel, giveTokenSuccess ? $"Hey @{username}, I've converted your Byte(s) to VIP(s) :D" : $"Hey @{username}, it looks like you don't have enough byte(s) to do that. Stick around and you'll have enough in no time!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will convert Bytes to VIPs if you have enough!");
        }
    }
}
