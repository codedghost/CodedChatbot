using System;

using Microsoft.EntityFrameworkCore;

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
