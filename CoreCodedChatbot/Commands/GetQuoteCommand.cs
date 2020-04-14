using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "quote" }, false)]
    public class GetQuoteCommand : ICommand
    {
        private readonly IQuoteApiClient _quoteApiClient;

        public GetQuoteCommand(IQuoteApiClient quoteApiClient)
        {
            _quoteApiClient = quoteApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var parsed = int.TryParse(commandText, out var quoteId);
            var request = new GetQuoteRequest
            {
                QuoteId = parsed ? quoteId : (int?)null
            };

            var quote = await _quoteApiClient.GetQuote(request);

            if (quote == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I had some trouble getting that Quote, please try again soon");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, Here is Quote {quote.QuoteId}: {quote.QuoteText}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will add a quote to the database!");
        }
    }
}