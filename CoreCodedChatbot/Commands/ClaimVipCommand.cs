using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.ApiContract.ResponseModels.Vip;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Interfaces;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "claimvip", "convertvip" }, false)]
    public class ClaimVipCommand : ICommand
    {
        private readonly ILogger<ClaimVipCommand> _logger;
        private readonly HttpClient _vipApiClient;

        public ClaimVipCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<ClaimVipCommand> logger)
        {
            _logger = logger;
            _vipApiClient = HttpClientHelper.BuildClient(configService, secretService, "Vip");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (!int.TryParse(commandText, out var numberOfTokens))
            {
                numberOfTokens = 1;
            }

            var request = new ConvertVipsRequest
            {
                Username = username,
                NumberOfBytes = numberOfTokens
            };
            ByteConversionResponse convertResponse;
            try
            {
                var result = await _vipApiClient.PostAsync("ConvertBytes", HttpClientHelper.GetJsonData(request));

                convertResponse = JsonConvert.DeserializeObject<ByteConversionResponse>(
                    await result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                convertResponse = HttpClientHelper.LogError<ByteConversionResponse>(_logger, e,
                    new object[] { request.Username, request.NumberOfBytes });
            }

            if (convertResponse == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, Sorry I can't do that at the moment, please try again in a few minutes");
                return;
            }

            client.SendMessage(joinedChannel, convertResponse.ConvertedBytes > 0 ? $"Hey @{username}, I've converted {convertResponse.ConvertedBytes} Byte(s) to VIP(s) :D" : $"Hey @{username}, it looks like you don't have enough byte(s) to do that. Stick around and you'll have enough in no time!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will convert Bytes to VIPs if you have enough!");
        }
    }
}
