using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Printful.ExternalClients;
using CoreCodedChatbot.Printful.Interfaces.ExternalClients;
using CoreCodedChatbot.Printful.Interfaces.Factories;

namespace CoreCodedChatbot.Printful.Factories
{
    public class PrintfulClientFactory : IPrintfulClientFactory
    {
        private ConfigModel _config;

        public PrintfulClientFactory(IConfigService configService)
        {
            _config = configService.GetConfig();
        }


        public IPrintfulClient Get()
        {
            return new PrintfulClient(_config.PrintfulAPIKey, _config.PrintfulAPIBaseUrl);
        }
    }
}