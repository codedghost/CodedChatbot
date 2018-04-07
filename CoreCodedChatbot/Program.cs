using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using CoreCodedChatbot.Services;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Helpers;

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

            var container = UnityHelper.Create();
            var chatbotService = container.Resolve<ChatbotService>();

            chatbotService.Main();

            Console.ReadLine();
        }
    }
}
