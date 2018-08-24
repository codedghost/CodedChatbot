using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using CoreCodedChatbot.Web.SignalRHubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using TwitchLib.Api;

namespace CoreCodedChatbot.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ConfigHelper().GetConfig();

            services.AddAuthentication(op =>
                {
                    op.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = TwitchAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTwitch(options =>
                {
                    options.ClientId = config.TwitchWebAppClientId;
                    options.ClientSecret = config.TwitchWebAppClientSecret;
                    options.Scope.Add(config.TwitchWebAppScopes);
                    options.CallbackPath = PathString.FromUriComponent($"{config.WebsiteLink}{config.TwitchWebAppCallbackPath}");
                });

            services.AddMvc();
            services.AddSignalR();

            services.AddSingleton<IChatbotContextFactory, ChatbotContextFactory>();
            services.AddSingleton<IConfigHelper, ConfigHelper>();
            services.AddSingleton<PlaylistHelper>();
            services.AddSingleton(typeof(SignalRHeartbeatService), typeof(SignalRHeartbeatService));
            services.AddSingleton<IChatterService, ChatterService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSignalR(routes =>
            {
                routes.MapHub<SongList>("/SongList");
            });

            var heartbeatService = serviceProvider.GetService<SignalRHeartbeatService>();
            var chatterService = serviceProvider.GetService<IChatterService>();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
