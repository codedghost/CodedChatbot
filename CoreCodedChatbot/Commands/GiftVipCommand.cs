﻿using System.Linq;
using System.Net;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
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

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandSplit = commandText.Split(" ");

            if (!commandSplit.Any() || !commandSplit[0].Contains("@"))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, you need to @ someone to gift a vip!");
                return;
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
