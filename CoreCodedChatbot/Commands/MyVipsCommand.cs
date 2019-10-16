using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "myvips", "mvip", "myvip", "vips"}, false)]
    public class MyVipsCommand : ICommand
    {
        private readonly IVipHelper vipHelper;

        public MyVipsCommand(IVipHelper vipHelper)
        {
            this.vipHelper = vipHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var vips = vipHelper.GetVipRequests(username);

            if (vips == null)
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, something went wrong with the chatbot. Ask @CodedGhost2 what he's playing at!");
                return;
            }

            client.SendMessage(joinedChannel,
                vips.TotalRemaining == 0
                    ? $"Hey @{username}, it looks like you have {vips.TotalRemaining}. :("
                    : $"Hey @{username}, it looks like you have {vips.TotalRemaining} VIPs left!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you how many VIP requests you still have!");
        }
    }
}
