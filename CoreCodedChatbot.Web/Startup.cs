using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AspNet.Security.OAuth.Twitch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Services;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Services;
using CoreCodedChatbot.Web.SignalRHubs;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = new ConfigService().GetConfig();

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
                    options.CallbackPath = PathString.FromUriComponent(config.TwitchWebAppCallbackPath);
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.ApiSecretSymmetricKey)),
                        ValidateIssuer = true,
                        ValidIssuer = config.ApiValidIssuer,
                        ValidateAudience = true,
                        ValidAudience = config.ApiValidAudience
                    };
                });

            services.AddMvc();

            services.AddSignalR();


            var creds = new ConnectionCredentials(config.ChatbotNick, config.ChatbotPass);
            var client = new TwitchClient();
            client.Initialize(creds, config.StreamerChannel);
            client.Connect();

            services.AddSingleton(client);
            services.AddSingleton<IChatbotContextFactory, ChatbotContextFactory>();
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<IGuessingGameService, GuessingGameService>();
            services.AddSingleton(typeof(SignalRHeartbeatService), typeof(SignalRHeartbeatService));
            services.AddSingleton<IChatterService, ChatterService>();
            services.AddSingleton<IPlaylistService, PlaylistService>();
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
