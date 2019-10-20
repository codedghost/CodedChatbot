using System;
using CoreCodedChatbot.ApiClient;
using CoreCodedChatbot.Database;
using Microsoft.EntityFrameworkCore;
using CoreCodedChatbot.Services;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library;
using Microsoft.Extensions.DependencyInjection;
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

            serviceProvider.GetService<CommandHelper>().Init(serviceProvider);

            var chatbotService = serviceProvider.GetService<IChatbotService>();

            chatbotService.Main();

            Console.ReadLine();
        }
    }
}
