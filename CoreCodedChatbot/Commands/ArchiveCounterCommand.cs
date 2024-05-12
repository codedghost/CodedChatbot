using System.Net.Http;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.RequestModels.Counters;
using CoreCodedChatbot.ApiContract.ResponseModels.Counters;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{

    [CustomAttributes.ChatCommand(new[] {"archiveoof", "archivecounter"}, true)]
    public class ArchiveCounterCommand : ICommand
    {
        private readonly ILogger<UpdateCounterSuffixCommand> _logger;
        private readonly HttpClient _counterApiClient;

        public ArchiveCounterCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<UpdateCounterSuffixCommand> logger)
        {
            _logger = logger;
            _counterApiClient = HttpClientHelper.BuildClient(configService, secretService, "Counters");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod,
            JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, you haven't provided a counter name");
                return;
            }

            var response = await _counterApiClient.PostAsync<ArchiveCounterRequest, ArchiveCounterResponse>(
                "archive",
                new ArchiveCounterRequest
                {
                    CounterName = commandText
                },
                _logger);

            if (response == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I couldn't archive that counter, please try again");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, The counter {response.CounterName} has been archived at {response.CounterValue}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will archive a counter and fetch the value for you!");
        }
    }
}