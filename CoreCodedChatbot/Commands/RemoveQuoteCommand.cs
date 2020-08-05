using System.Linq;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "removequote", "rq" }, false)]
    public class RemoveQuoteCommand : ICommand
    {
        private readonly IQuoteApiClient _quoteApiClient;

        public RemoveQuoteCommand(IQuoteApiClient quoteApiClient)
        {
            _quoteApiClient = quoteApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandTerms = commandText.Split(" ");
            if (!int.TryParse(commandTerms[0], out var quoteId))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, looks like you didn't provide a Quote number for me to remove!");
                return;
            }

            var response = await _quoteApiClient.RemoveQuote(new RemoveQuoteRequest
            {
                QuoteId = quoteId,
                Username = username,
                IsMod = isMod
            });

            client.SendMessage(joinedChannel,
                response
                    ? $"Hey @{username}, I have removed that quote for you!"
                    : $"Hey @{username}, I'm sorry I couldn't remove that quote. Is that one you created?");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will allow you to remove your quotes! Usage: !removequote 3 (quote number)");
        }
    }
}