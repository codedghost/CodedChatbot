using System;
using System.Linq;
using System.Text;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Database;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using CoreCodedChatbot.Library;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Services;
using CoreCodedChatbot.Web.SignalRHubs;
using Microsoft.Extensions.Hosting;

namespace CoreCodedChatbot.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var configService = new ConfigService();
            var secretService = new AzureKeyVaultService(configService);
            secretService.Initialize().Wait();

            services.AddOptions();
            services.AddMemoryCache();

            //var builder = new ConfigurationBuilder().SetBasePath(Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT"));
            //var configuration = builder.Build();

            //services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            //services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            //services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            //.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            services.AddChatbotWebAuth(configService, secretService);

            services.AddMvc();

            services.AddSignalR();

            //api.V5.Chat.GetChatRoomsByChannelAsync(config.ChannelId, config.ChatbotAccessToken)
            //    .ContinueWith(
            //        rooms =>
            //        {
            //            if (!rooms.IsCompletedSuccessfully) return;
            //            var devRoomId = rooms.Result.Rooms.SingleOrDefault(r => r.Name == "dev")?.Id;
            //            if (!string.IsNullOrWhiteSpace(devRoomId))
            //            {
            //                client.JoinRoom(config.ChannelId, devRoomId);
            //            }
            //        });

            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddTwitchServices(configService, secretService)
                .AddDbContextFactory()
                .AddLibraryServices()
                .AddSignalRServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //app.UseIpRateLimiting();

            app.UseStaticFiles();

            var heartbeatService = serviceProvider.GetService<SignalRHeartbeatService>();
            var chatterService = serviceProvider.GetService<IChatterService>();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHub<SongList>("/SongList");
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
