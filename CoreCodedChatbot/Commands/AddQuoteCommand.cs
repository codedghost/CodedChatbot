using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] {"addquote"}, false)]
    public class AddQuoteCommand : ICommand
    {
        private readonly IQuoteApiClient _quoteApiClient;

        public AddQuoteCommand(IQuoteApiClient quoteApiClient)
        {
            _quoteApiClient = quoteApiClient;
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

            var quote = await _quoteApiClient.AddQuote(request);

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