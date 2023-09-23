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
    [CustomAttributes.ChatCommand(new[] { "quote" }, false)]
    public class GetQuoteCommand : ICommand
    {
        private readonly ILogger<GetQuoteCommand> _logger;
        private readonly HttpClient _quoteApiClient;

        public GetQuoteCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<GetQuoteCommand> logger)
        {
            _logger = logger;
            _quoteApiClient = HttpClientHelper.BuildClient(configService, secretService, "Quote");
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var parsed = int.TryParse(commandText, out var quoteId);
            var request = new GetQuoteRequest
            {
                QuoteId = parsed ? quoteId : (int?)null
            };

            var quote = await _quoteApiClient.GetAsync<GetQuoteResponse>("GetQuote" + 
                (request.QuoteId.HasValue ? $"?quoteId={request.QuoteId.Value}" : string.Empty),
                _logger);

            if (quote == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I had some trouble getting that Quote, please try again soon");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, Here is Quote {quote.Quote.QuoteId}: {quote.Quote.QuoteText}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will add a quote to the database!");
        }
    }
}