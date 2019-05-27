using System;
using CoreCodedChatbot.CF.Helpers;
using CoreCodedChatbot.CF.Services;
using CoreCodedChatbot.Database.Context;
using Microsoft.EntityFrameworkCore;
using Unity;

namespace CoreCodedChatbot.CF
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CF download service started");

            using (var context = new ChatbotContext())
            {
                context.Database.Migrate();
            }

            var container = UnityHelper.Create();
            var cfService = container.Resolve<CFService>();

            if (cfService.Main()) Console.ReadLine();
        }
    }
}
