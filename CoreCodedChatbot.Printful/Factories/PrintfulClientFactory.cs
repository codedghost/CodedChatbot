using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Printful.ExternalClients;
using CoreCodedChatbot.Printful.Interfaces.ExternalClients;
using CoreCodedChatbot.Printful.Interfaces.Factories;

namespace CoreCodedChatbot.Printful.Factories
{
    public class PrintfulClientFactory : IPrintfulClientFactory
    {
        private readonly IConfigService _configService;
        private readonly ISecretService _secretService;

        public PrintfulClientFactory(IConfigService configService, ISecretService secretService)
        {
            _configService = configService;
            _secretService = secretService;
        }


        public IPrintfulClient Get()
        {
            return new PrintfulClient(_secretService.GetSecret<string>("PrintfulAPIKey"),
                _configService.Get<string>("PrintfulAPIBaseUrl"));
        }
    }
}