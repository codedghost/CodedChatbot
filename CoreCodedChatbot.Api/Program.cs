using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Database.Context;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreCodedChatbot.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new ChatbotContext())
            {
                context.Database.Migrate();
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: true)
                .Build();

            CreateWebHostBuilder(args, config).Run();
        }

        public static IWebHost CreateWebHostBuilder(string[] args, IConfigurationRoot config) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .Build();
    }
}
