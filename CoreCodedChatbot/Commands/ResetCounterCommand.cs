using System.Net.Http;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.RequestModels.Counters;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Extensions;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "resetOofs", "resetCounter" }, true)]
    public class ResetCounterCommand : ICommand
    {
        private readonly ILogger<ResetCounterCommand> _logger;
        private readonly HttpClient _counterApiClient;

        public ResetCounterCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<ResetCounterCommand> logger)
        {
            _logger = logger;
            _counterApiClient = HttpClientHelper.BuildClient(configService, secretService, "Counters");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandTerms = commandText.SplitCommandText();
            if (commandTerms.Length > 2)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, looks like you haven't provided a valid counter name");
                return;
            }

            var response = await _counterApiClient.PostAsync<ResetCounterRequest, bool>("Reset",
                new ResetCounterRequest
                {
                    CounterName = commandText
                }, _logger);

            client.SendMessage(joinedChannel,
                response
                    ? $"Hey @{username}, I have reset the Counter: {commandText}!"
                    : $"Hey @{username}, I'm sorry I couldn't reset that counter. Please try again soon");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will reset any oof/counter to zero. Usage: !resetOofs <counterName>");
        }
    }
}