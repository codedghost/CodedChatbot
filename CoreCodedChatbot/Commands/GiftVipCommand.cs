using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.Extensions;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "giftvip", "iamasaintto"}, false)]
    public class GiftVipCommand : ICommand
    {
        private readonly IVipApiClient _vipApiClient;

        public GiftVipCommand(IVipApiClient vipApiClient)
        {
            _vipApiClient = vipApiClient;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandSplit = commandText.SplitCommandText();

            if (!commandSplit.Any() || !commandSplit[0].Contains("@"))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, you need to @ someone to gift a vip!");
                return;
            }

            if (commandSplit.Length > 2)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, you've provided too much info, run !help giftvip to see how this command works");
            }

            var numberOfVipsGiven = commandSplit.Length == 2;


            var giftVipModel = new GiftVipRequest
            {
                DonorUsername = username,
                ReceiverUsername = commandSplit[0].Trim('@'),
                NumberOfVips = numberOfVipsGiven ? 
                    int.TryParse(commandSplit[1].Trim(), out var numberOfVips) 
                        ? numberOfVips : 1 
                    : 1
            };
            
            var giftVipResult = await _vipApiClient.GiftVip(giftVipModel);

            if (giftVipResult)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I have given @{giftVipModel.ReceiverUsername} {giftVipModel.NumberOfVips} of your VIPs");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, I couldn't give the VIP for some reason, do you have enough VIPs or did you mistype the @?");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command lets you gift a vip to another viewer. Usage (remove <>): !giftvip @<username> <numberofvipstogive>");
        }
    }
}
