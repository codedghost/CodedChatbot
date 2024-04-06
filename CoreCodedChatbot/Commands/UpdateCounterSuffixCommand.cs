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
    [CustomAttributes.ChatCommand(new [] { "updateOofText", "updateCounterText" }, true)]
    public class UpdateCounterSuffixCommand : ICommand
    {
        private readonly ILogger<UpdateCounterSuffixCommand> _logger;
        private readonly HttpClient _counterApiClient;

        public UpdateCounterSuffixCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<UpdateCounterSuffixCommand> logger)
        {
            _logger = logger;
            _counterApiClient = HttpClientHelper.BuildClient(configService, secretService, "Counters");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandTerms = commandText.SplitCommandText(" - ");
            if (commandTerms.Length != 2)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, looks like you haven't provided a valid counter name and new suffix");
                return;
            }

            var response = await _counterApiClient.PostAsync<UpdateCounterSuffixRequest, bool>("UpdateSuffix",
                new UpdateCounterSuffixRequest()
                {
                    CounterName = commandTerms[0],
                    CounterSuffix = commandTerms[1]
                }, _logger);

            client.SendMessage(joinedChannel,
                response
                    ? $"Hey @{username}, I have updated the Counter: {commandText}!"
                    : $"Hey @{username}, I'm sorry I couldn't update that counter. Please try again soon");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will update the suffix of any specified counter. Usage: !resetOofs <counterName> - <counterSuffix>");
        }
    }
}