using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Commands;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Services;
using CoreCodedChatbot.Secrets;
using CoreCodedChatbot.Services;
using Microsoft.Extensions.DependencyInjection;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

namespace CoreCodedChatbot
{
    public static class Package
    {
        public static IServiceCollection AddTwitchServices(this IServiceCollection services)
        {
            var configService = new ConfigService();
            var secretService = new AzureKeyVaultService(
                configService.Get<string>("KeyVaultAppId"),
                configService.Get<string>("KeyVaultCertThumbprint"),
                configService.Get<string>("KeyVaultBaseUrl")
                );
            secretService.Initialize().Wait();

            var creds = new ConnectionCredentials(configService.Get<string>("ChatbotNick"), secretService.GetSecret<string>("ChatbotPass"));
            var client = new TwitchClient();
            client.Initialize(creds, configService.Get<string>("StreamerChannel"));

            var api = new TwitchAPI();
            api.Settings.AccessToken = secretService.GetSecret<string>("ChatbotAccessToken");

            var liveStreamMonitor = new LiveStreamMonitorService(api);

            var pubsub = new TwitchPubSub();

            // Register all external Twitch services
            services.AddSingleton(api);
            services.AddSingleton(client);
            services.AddSingleton(pubsub);
            services.AddSingleton(liveStreamMonitor);

            return services;
        }

        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            // Register chatbot helpers (these should really be services)
            services.AddTransient<IRedditHelper, RedditHelper>();
            services.AddSingleton<IBytesHelper, BytesHelper>();
            services.AddSingleton<IHelpHelper, HelpHelper>();
            services.AddSingleton<IStreamLabsHelper, StreamLabsHelper>();
            services.AddSingleton<IVipHelper, VipHelper>();

            return services;
        }

        public static IServiceCollection AddChatCommands(this IServiceCollection services)
        {
            services.AddTransient<AddInfoCommand>();
            services.AddTransient<AwwCommand>();
            services.AddTransient<ClaimAllVipsCommand>();
            services.AddTransient<ClaimVipCommand>();
            services.AddTransient<ClosePlaylistCommand>();
            services.AddTransient<DiscordCommand>();
            services.AddTransient<EditRockRequestCommand>();
            services.AddTransient<FollowageCommand>();
            services.AddTransient<GiftVipCommand>();
            services.AddTransient<GiveVipCommand>();
            services.AddTransient<GuessCommand>();
            services.AddTransient<HelpCommand>();
            services.AddTransient<HowToRequestCommand>();
            services.AddTransient<InfoCommand>();
            services.AddTransient<InstagramCommand>();
            services.AddTransient<KittyCommand>();
            services.AddTransient<MerchCommand>();
            services.AddTransient<MyBytesCommand>();
            services.AddTransient<MyRockRequestsCommand>();
            services.AddTransient<MyVipsCommand>();
            services.AddTransient<OpenPlaylistCommand>();
            services.AddTransient<PromoteCommand>();
            services.AddTransient<RemoveCurrentRequestCommand>();
            services.AddTransient<RemoveRockRequestCommand>();
            services.AddTransient<RemoveSuperVipCommand>();
            services.AddTransient<RockRequestCommand>();
            services.AddTransient<RocksmithChallengeCommand>();
            services.AddTransient<ShoutoutCommand>();
            services.AddTransient<SocialCommand>();
            services.AddTransient<SongListCommand>();
            services.AddTransient<SuperVipCommand>();
            services.AddTransient<TwitterCommand>();
            services.AddTransient<UptimeCommand>();
            services.AddTransient<VipCommand>();
            services.AddTransient<YoutubeCommand>();

            // Register CommandHelper
            services.AddSingleton<ICommandHelper, CommandHelper>();

            return services;
        }

        public static IServiceCollection AddChatbotServices(this IServiceCollection services)
        {
            services.AddSingleton<IChatbotService, ChatbotService>();

            return services;
        }
    }
}