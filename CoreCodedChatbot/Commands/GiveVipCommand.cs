using System.Linq;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "gvip", "givevip" }, true)]
    public class GiveVipCommand : ICommand
    {
        private readonly VipHelper vipHelper;

        private readonly ConfigModel config;

        public GiveVipCommand(VipHelper vipHelper, ConfigModel config)
        {
            this.vipHelper = vipHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var splitCommandText = commandText.Split(" ");

            if (commandText.Contains("@"))
            {
                if (splitCommandText.Length == 1)
                {
                    client.SendMessage(joinedChannel,
                        vipHelper.GiveVipRequest(commandText.TrimStart('@'))
                            ? $"Hey @{username}, I have successfully given {commandText} a VIP request!"
                            : $"Hey @{username}, sorry something seems to be wrong here. Please check your command usage. Type !help gvip for more detailed help");
                } else if (splitCommandText.Length == 2)
                {
                    var giveUser = splitCommandText.SingleOrDefault(x => x.Contains("@")).TrimStart('@');
                    var giveAmount = 0;
                    int.TryParse(splitCommandText.SingleOrDefault(x => !x.Contains("@")), out giveAmount);
                    for (int i = 0; i < giveAmount; i++)
                    {
                        if (!vipHelper.GiveVipRequest(giveUser))
                        {
                            client.SendMessage(joinedChannel, $"Hey @{username}, sorry something seems to be wrong here. I managed to give {i} VIPs. Please check your command usage. Type !help gvip for more detailed help");
                        }
                    }
                    client.SendMessage(joinedChannel, $"Hey @{username}, I have successfully given @{giveUser} {giveAmount} VIPs");
                }
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command is used by moderators to give out VIP requests. Hint: Ensure you use the '@'. Usage: !gvip <username> <optionalAmount>");
        }
    }
}
