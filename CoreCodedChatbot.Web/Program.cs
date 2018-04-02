
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

using CoreCodedChatbot.Database.Context;


namespace CoreCodedChatbot.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new ChatbotContext())
            {
                context.Database.Migrate();
            }

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
