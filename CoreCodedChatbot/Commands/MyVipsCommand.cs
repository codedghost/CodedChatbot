using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "myvips", "mvip", "myvip", "vips"}, false)]
    public class MyVipsCommand : ICommand
    {
        private readonly VipHelper vipHelper;

        private readonly ConfigModel config;

        public MyVipsCommand(VipHelper vipHelper, ConfigModel config)
        {
            this.vipHelper = vipHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var vips = vipHelper.GetVipRequests(username);

            if (vips == null)
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, something went wrong with the chatbot. Ask @CodedGhost2 what he's playing at!");
                return;
            }

            client.SendMessage(config.StreamerChannel,
                vips.TotalRemaining == 0
                    ? $"Hey @{username}, it looks like you have {vips.TotalRemaining}. :("
                    : $"Hey @{username}, it looks like you have {vips.TotalRemaining} VIPs left!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will tell you how many VIP requests you still have!");
        }
    }
}
