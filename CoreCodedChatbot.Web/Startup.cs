using System;
using System.Linq;
using System.Text;
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
using TwitchLib.Api;
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

            services.AddOptions();
            services.AddMemoryCache();

            //var builder = new ConfigurationBuilder().SetBasePath(Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOT"));
            //var configuration = builder.Build();

            //services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            //services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            //services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            //.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

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

            var api = new TwitchAPI();
            api.Settings.AccessToken = config.ChatbotAccessToken;


            api.V5.Chat.GetChatRoomsByChannelAsync(config.ChannelId, config.ChatbotAccessToken)
                .ContinueWith(
                    rooms =>
                    {
                        if (!rooms.IsCompletedSuccessfully) return;
                        var devRoomId = rooms.Result.Rooms.SingleOrDefault(r => r.Name == "dev")?.Id;
                        if (!string.IsNullOrWhiteSpace(devRoomId))
                        {
                            client.JoinRoom(config.ChannelId, devRoomId);
                        }
                    });

            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddSingleton(client);
            services.AddSingleton(api);
            services.AddSingleton<IChatbotContextFactory, ChatbotContextFactory>();
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<IVipService, VipService>();
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

            //app.UseIpRateLimiting();

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
