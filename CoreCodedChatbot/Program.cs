using System;

using Microsoft.EntityFrameworkCore;

using CoreCodedChatbot.Services;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Helpers;
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
                .AddChatCommands()
                .BuildServiceProvider();

            serviceProvider.GetService<CommandHelper>().Init(serviceProvider);

            var chatbotService = serviceProvider.GetService<ChatbotService>();

            chatbotService.Main();

            Console.ReadLine();
        }
    }
}
