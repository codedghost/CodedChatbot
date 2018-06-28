using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Discord.Helpers;
using CoreCodedChatbot.Discord.Helpers.Interfaces;
using CoreCodedChatbot.Discord.Services;
using CoreCodedChatbot.Discord.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreCodedChatbot.Discord
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public async void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IConfigHelper, ConfigHelper>();
            
            services.AddSingleton<IDiscordService, DiscordService>(provider =>
            {
                var discordService = new DiscordService(provider.GetRequiredService<IConfigHelper>());
                discordService.Initialise();
                return discordService;
            });

            services.BuildServiceProvider().GetService<IDiscordService>(); // Initialise the service
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
