using System;
using System.Linq;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.ApiRequest.Vip;
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

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandSplit = commandText.Split(" ");

            if (!commandSplit.Any() || commandSplit.Length != 1 || !commandSplit[0].Contains("@"))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, you need to @ someone to gift a vip!");
                return;
            }

            var giftVipModel = new GiftVipModel
            {
                DonorUsername = username,
                ReceiverUsername = commandSplit[0].Trim('@')
            };
            
            var giftVipResult = await _vipApiClient.GiftVip(giftVipModel);

            if (giftVipResult)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I have given @{giftVipModel.ReceiverUsername} one of your VIPs");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, I couldn't give the VIP for some reason, did you mistype the @?");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command lets you gift a vip to another viewer. Usage: !giftvip @<username>");
        }
    }
}
