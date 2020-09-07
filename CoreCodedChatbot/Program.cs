using System;
using CodedGhost.Config;
using CoreCodedChatbot.ApiClient;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Logging;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodedChatbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Must be a better way of doing this
            var config = new ConfigService();

            var serviceProvider = new ServiceCollection()
                .AddChatbotConfigService()
                .AddChatbotSecretServiceCollection(
                    config.Get<string>("KeyVaultAppId"),
                    config.Get<string>("KeyVaultCertThumbprint"),
                    config.Get<string>("KeyVaultBaseUrl")
                )
                .AddChatbotNLog()
                .AddApiClientServices()
                .AddTwitchServices()
                .AddHelpers()
                .AddChatCommands()
                .AddChatbotServices()
                .BuildServiceProvider();

            // Set up Unhandled error logging
            Logging.Package.ConfigureNLogForConsoleApp<Program>(serviceProvider);

            var commandHelper = serviceProvider.GetService<ICommandHelper>();
            commandHelper.Init(serviceProvider);

            var chatbotService = serviceProvider.GetService<IChatbotService>();

            chatbotService.Main();

            Console.ReadLine();
        }
    }
}
