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
    [CustomAttributes.ChatCommand(new[] { "getoof", "getcounter" }, false)]
    public class GetCounterCommand : ICommand
    {
        private readonly ILogger<UpdateCounterSuffixCommand> _logger;
        private readonly HttpClient _counterApiClient;

        public GetCounterCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<UpdateCounterSuffixCommand> logger)
        {
            _logger = logger;
            _counterApiClient = HttpClientHelper.BuildClient(configService, secretService, "Counters");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, you haven't provided a counter name");
                return;
            }

            var response =
                await _counterApiClient.GetAsync<GetCounterResponse>($"GetCounter?counterName={commandText}", _logger);

            if (response == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I couldn't retrieve that counter, please try again");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, Here's the counter {response.Counter.CounterName}: {response.Counter.CounterPreText}: {response.Counter.CounterValue}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will fetch a counter and it's value for you!");
        }
    }
}