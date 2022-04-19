using System.Linq;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.Extensions;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "editquote", "eq" }, false)]
    public class EditQuoteCommand : ICommand
    {
        private readonly IQuoteApiClient _quoteApiClient;

        public EditQuoteCommand(IQuoteApiClient quoteApiClient)
        {
            _quoteApiClient = quoteApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandTerms = commandText.SplitCommandText();
            if (!int.TryParse(commandTerms[0], out var quoteId))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, looks like you didn't provide a Quote number for me to edit!");
                return;
            }

            var newText = string.Join(" ", commandTerms.Skip(1));

            var response = await _quoteApiClient.EditQuote(new EditQuoteRequest
            {
                QuoteId = quoteId,
                QuoteText = newText,
                Username = username,
                IsMod = isMod
            });

            client.SendMessage(joinedChannel,
                response
                    ? $"Hey @{username}, I have updated that quote for you!"
                    : $"Hey @{username}, I'm sorry I couldn't update that quote. Is that one you created?");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will allow you to edit your quotes! Usage (remove <>): <quoteId> <new quote text>");
        }
    }
}