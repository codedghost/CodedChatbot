using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.Vip;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "giftvip", "iamasaintto"}, false)]
    public class GiftVipCommand : ICommand
    {
        private ConfigModel config;
        private HttpClient vipClient;

        public GiftVipCommand(ConfigModel config)
        {
            this.config = config;

            this.vipClient = new HttpClient
            {
                BaseAddress = new Uri(config.VipApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var commandSplit = commandText.Split(" ");

            if (!commandSplit.Any() || commandSplit.Length != 1 || !commandSplit[0].Contains("@"))
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, you need to @ someone to gift a vip!");
                return;
            }

            var giftVipModel = new GiftVipModel
            {
                DonorUsername = username,
                ReceiverUsername = commandSplit[0].Trim('@')
            };
            
            var giftVipResult = await vipClient.PostAsync("GiftVip",
                HttpClientHelper.GetJsonData(giftVipModel));

            if (giftVipResult.IsSuccessStatusCode)
            {
                client.SendMessage(config.StreamerChannel,
                    $"Hey @{username}, I have given @{giftVipModel.ReceiverUsername} one of your VIPs");
                return;
            }

            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, I couldn't give the VIP for some reason, did you mistype the @?");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, this command lets you gift a vip to another viewer. Usage: !giftvip @<username>");
        }
    }
}
