using System;
using System.Security.Cryptography.X509Certificates;
using CoreCodedChatbot.ApiClient;
using CoreCodedChatbot.Commands;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Database;
using Microsoft.EntityFrameworkCore;
using CoreCodedChatbot.Services;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodedChatbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigService();

            var serviceProvider = new ServiceCollection()
                .AddChatbotConfigService()
                .AddChatbotSecretServiceCollection(
                    config.Get<string>("KeyVaultAppId"),
                    config.Get<string>("KeyVaultCertThumbprint"),
                    config.Get<string>("KeyVaultBaseUrl")
                )
                .AddTwitchServices()
                .AddLibraryServices()
                .AddHelpers()
                .AddApiClientServices()
                .AddChatCommands()
                .AddChatbotServices()
                .AddDbContextFactory()
                .BuildServiceProvider();

            using (var context = new ChatbotContext())
            {
                context.Database.Migrate();
            }


            var commandHelper = serviceProvider.GetService<ICommandHelper>();
            commandHelper.Init(serviceProvider);

            var chatbotService = serviceProvider.GetService<IChatbotService>();

            chatbotService.Main();

            Console.ReadLine();
        }
    }
}
