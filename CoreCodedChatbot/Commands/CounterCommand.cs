using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
    [CustomAttributes.ChatCommand(new[] { "oof", "counter" }, true)]
    public class CounterCommand : ICommand
    {
        private readonly ILogger<AddQuoteCommand> _logger;
        private readonly HttpClient _counterApiClient;

        private readonly Regex _counterRegex =
            new Regex("^(?<counterName>[a-zA-Z0-9 ]+){1}(- )?(?<preText>[a-zA-Z0-9 ]+)?(- )?(?<startIndex>\\d+)?", RegexOptions.Multiline);

        public CounterCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<AddQuoteCommand> logger)
        {
            _logger = logger;
            _counterApiClient = HttpClientHelper.BuildClient(configService, secretService, "Counters");
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, I need to know which counter I'm updating!");
                return;
            }

            var match = _counterRegex.Match(commandText);
            if (!match.Success)
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, I couldn't figure out what you were asking me to do! Try '!help counter' for more info");
                return;
            }

            var counterName = string.Empty;
            string? counterSuffix = default;
            int? initialValue = default;

            var requestUri = new StringBuilder("UpdateCounter");

            if (match.Groups["counterName"].Success)
            {
                counterName = match.Groups["counterName"].Value.Trim();

                requestUri.Append($"?counterName={counterName}");
            }

            if (match.Groups["preText"].Success)
            {
                counterSuffix = match.Groups["preText"].Value.Trim();
            }

            if (match.Groups["startIndex"].Success)
            {
                initialValue = int.Parse(match.Groups["startIndex"].Value.Trim());
            }
            

            if (counterSuffix != null || initialValue != null)
            {
                var success = await _counterApiClient.PutAsync<CreateCounterRequest, bool>("AddCounter",
                    new CreateCounterRequest
                    {
                        CounterName = counterName,
                        CounterPreText = counterSuffix,
                        CounterInitialVal = initialValue
                    }, _logger);

                if (success)
                {
                    client.SendMessage(joinedChannel, $"Hey @{username}, I've updated the counter");
                    return;
                }
                else
                {
                    client.SendMessage(joinedChannel, $"Sorry @{username}, something went wrong :(");
                    return;
                }
            }

            var updateResult =
                await _counterApiClient.GetAsync<UpdateCounterResponse>($"UpdateCounter?counterName={counterName}", _logger);

            if (updateResult != null)
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, I've updated the counter, the new count is: {updateResult.Counter.CounterValue}");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will update a counter! !counter <counter name> - <text suffix> - <initial value>");
        }
    }
}