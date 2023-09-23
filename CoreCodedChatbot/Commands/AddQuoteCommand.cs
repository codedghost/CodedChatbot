using System.Net.Http;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.ApiContract.ResponseModels.Quotes;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] {"addquote"}, false)]
    public class AddQuoteCommand : ICommand
    {
        private readonly ILogger<AddQuoteCommand> _logger;
        private readonly HttpClient _quoteApiClient;

        public AddQuoteCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<AddQuoteCommand> logger)
        {
            _logger = logger;
            _quoteApiClient = HttpClientHelper.BuildClient(configService, secretService, "Quote");
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, Please enter some quote text!");
                return;
            }

            var request = new AddQuoteRequest
            {
                QuoteText = commandText,
                Username = username
            };

            var quote = await _quoteApiClient.PutAsync<AddQuoteRequest, AddQuoteResponse>("AddQuote", request, _logger);

            if (quote == null)
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I had some trouble adding that to the database, please try again soon");

            client.SendMessage(joinedChannel,
                $"Hey @{username}, I have added Quote {quote.QuoteId}: {commandText}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will add a quote to the database!");
        }
    }
}