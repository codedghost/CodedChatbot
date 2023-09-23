using System.Net.Http;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.RequestModels.Quotes;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Extensions;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "removequote", "rq" }, false)]
    public class RemoveQuoteCommand : ICommand
    {
        private readonly ILogger<RemoveQuoteCommand> _logger;
        private readonly HttpClient _quoteApiClient;

        public RemoveQuoteCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<RemoveQuoteCommand> logger)
        {
            _logger = logger;
            _quoteApiClient = HttpClientHelper.BuildClient(configService, secretService, "Quote");
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandTerms = commandText.SplitCommandText();
            if (!int.TryParse(commandTerms[0], out var quoteId))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, looks like you didn't provide a Quote number for me to remove!");
                return;
            }

            var response = await _quoteApiClient.PostAsync<RemoveQuoteRequest, bool>("RemoveQuote", new RemoveQuoteRequest
            {
                QuoteId = quoteId,
                Username = username,
                IsMod = isMod
            }, _logger);

            client.SendMessage(joinedChannel,
                response
                    ? $"Hey @{username}, I have removed that quote for you!"
                    : $"Hey @{username}, I'm sorry I couldn't remove that quote. Is that one you created?");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will allow you to remove your quotes! Usage (remove <>): !removequote <quiteId>");
        }
    }
}