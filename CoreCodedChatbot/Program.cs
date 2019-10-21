using System;
using System.Security.Cryptography.X509Certificates;
using CoreCodedChatbot.ApiClient;
using CoreCodedChatbot.Commands;
using CoreCodedChatbot.Database;
using Microsoft.EntityFrameworkCore;
using CoreCodedChatbot.Services;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Unity;

namespace CoreCodedChatbot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new ChatbotContext())
            {
                context.Database.Migrate();
            }

            var serviceProvider = new ServiceCollection()
                .AddTwitchServices()
                .AddLibraryServices()
                .AddHelpers()
                .AddApiClientServices()
                .AddChatCommands()
                .AddChatbotServices()
                .AddDbContextFactory()
                .BuildServiceProvider();
            
            var commandHelper = serviceProvider.GetService<ICommandHelper>();
            commandHelper.Init(serviceProvider);

            var chatbotService = serviceProvider.GetService<IChatbotService>();

            chatbotService.Main();

            Console.ReadLine();
        }
    }
}
