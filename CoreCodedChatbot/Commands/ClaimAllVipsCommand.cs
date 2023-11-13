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
    [CustomAttributes.ChatCommand(new []{ "claimallvips", "claimall"}, false)]
    public class ClaimAllVipsCommand : ICommand
    {
        private readonly ILogger<ClaimAllVipsCommand> _logger;
        private readonly HttpClient _vipApiClient;
        
        public ClaimAllVipsCommand(
            IConfigService configServce,
            ISecretService secretService,
            ILogger<ClaimAllVipsCommand> logger)
        {
            _logger = logger;
            _vipApiClient = HttpClientHelper.BuildClient(configServce, secretService, "Vip");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            ByteConversionResponse response;
            var request = new ConvertAllVipsRequest
            {
                Username = username
            };

            try
            {
                var result = await _vipApiClient.PostAsync("ConvertAllBytes", HttpClientHelper.GetJsonData(request));

                response = JsonConvert.DeserializeObject<ByteConversionResponse>(
                    await result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                response = HttpClientHelper.LogError<ByteConversionResponse>(_logger, e, new object[] { request.Username });
            }

            if (response == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, Sorry I can't do that at the moment, please try again in a few minutes");
                return;
            }

            client.SendMessage(joinedChannel, response.ConvertedBytes > 0 ? $"Hey @{username}, I've converted {response.ConvertedBytes} Byte(s) to VIP(s) :D" : $"Hey @{username}, it looks like you don't have enough byte(s) to do that. Stick around and you'll have enough in no time!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, "This command will convert all of your bytes to VIP tokens!");
        }
    }
}
