using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "myvips", "mvip", "myvip", "vips"}, false)]
    public class MyVipsCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var vips = VipHelper.GetVipRequests(username);

            if (vips == null) client.SendMessage($"Hey @{username}, something went wrong with the chatbot. Ask @CodedGhost2 what he's playing at!");

            if (vips.TotalRemaining == 0) client.SendMessage($"Hey @{username}, it looks like you have {vips.TotalRemaining}. :(");
            else client.SendMessage($"Hey @{username}, it looks like you have {vips.TotalRemaining} VIPs left!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command will tell you how many VIP requests you still have!");
        }
    }
}
