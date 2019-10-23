using System.Text;
using AspNet.Security.OAuth.Twitch;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Web.Interfaces;
using CoreCodedChatbot.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Web
{
    public static class Package
    {
        public static IServiceCollection AddChatbotWebAuth(
            this IServiceCollection services, 
            IConfigService configService, 
            ISecretService secretService
            )
        {
            services.AddAuthentication(op =>
                {
                    op.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    op.DefaultChallengeScheme = TwitchAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddTwitch(options =>
                {
                    options.ClientId = secretService.GetSecret<string>("TwitchWebAppClientId");
                    options.ClientSecret = secretService.GetSecret<string>("TwitchWebAppClientSecret");
                    options.Scope.Add(configService.Get<string>("TwitchWebAppScopes"));
                    options.CallbackPath = PathString.FromUriComponent(configService.Get<string>("TwitchWebAppCallbackPath"));
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretService.GetSecret<string>("ApiSecretSymmetricKey"))),
                        ValidateIssuer = true,
                        ValidIssuer = secretService.GetSecret<string>("ApiValidIssuer"),
                        ValidateAudience = true,
                        ValidAudience = secretService.GetSecret<string>("ApiValidAudience")
                    };
                });

            return services;
        }

        public static IServiceCollection AddTwitchServices(
            this IServiceCollection services,
            IConfigService configService,
            ISecretService secretService
        )
        {
            var creds = new ConnectionCredentials(secretService.GetSecret<string>("ChatbotNick"), configService.Get<string>("ChatbotPass"));
            var client = new TwitchClient();
            client.Initialize(creds, configService.Get<string>("StreamerChannel"));
            client.Connect();

            var api = new TwitchAPI();
            api.Settings.AccessToken = secretService.GetSecret<string>("ChatbotAccessToken");

            services.AddSingleton(client);
            services.AddSingleton(api);

            services.AddSingleton<IChatterService, ChatterService>();

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(SignalRHeartbeatService), typeof(SignalRHeartbeatService));

            return services;
        }
    }
}