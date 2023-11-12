using System;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using CodedGhost.Config;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.ClientTrigger;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "background", "music", "song" }, false)]
    public class BackgroundSongCommand : ICommand
    {
        private readonly ILogger<BackgroundSongCommand> _logger;
        private readonly HttpClient _clientTriggerClient;

        public BackgroundSongCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<BackgroundSongCommand> logger)
        {
            _logger = logger;
            _clientTriggerClient = HttpClientHelper.BuildClient(configService, secretService, "ClientTrigger");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {

            bool result;
            var request = new CheckBackgroundSongRequest
            {
                Username = username
            };

            try
            {
                var response = await _clientTriggerClient.PostAsync("CheckBackgroundSong", HttpClientHelper.GetJsonData(request));

                result = response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                result = HttpClientHelper.LogError<bool>(_logger, e, new object[] { request.Username });
            }

            if (!result)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, sorry but I can't do that right now. Please try again in a few minutes");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will tell you the current background song playing!");
        }
    }
}