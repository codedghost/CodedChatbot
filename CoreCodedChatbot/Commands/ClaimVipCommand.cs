using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "claimvip", "convertvip" }, false)]
    public class ClaimVipCommand : ICommand
    {
        private readonly BytesHelper bytesHelper;


        public ClaimVipCommand(BytesHelper bytesHelper)
        {
            this.bytesHelper = bytesHelper;
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
