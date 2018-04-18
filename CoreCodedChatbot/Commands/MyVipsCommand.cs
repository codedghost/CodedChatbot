using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "myvips", "mvip", "myvip", "vips"}, false)]
    public class MyVipsCommand : ICommand
    {
        private readonly VipHelper vipHelper;

        private readonly ConfigModel config = ConfigHelper.GetConfig();

        public MyVipsCommand(VipHelper vipHelper)
        {
            this.vipHelper = vipHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var vips = vipHelper.GetVipRequests(username);

            if (vips == null) client.SendMessage(config.StreamerChannel, $"Hey @{username}, something went wrong with the chatbot. Ask @CodedGhost2 what he's playing at!");

            if (vips.TotalRemaining == 0) client.SendMessage(config.StreamerChannel, $"Hey @{username}, it looks like you have {vips.TotalRemaining}. :(");
            else client.SendMessage(config.StreamerChannel, $"Hey @{username}, it looks like you have {vips.TotalRemaining} VIPs left!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will tell you how many VIP requests you still have!");
        }
    }
}
