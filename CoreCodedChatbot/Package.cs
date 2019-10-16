using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Commands;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Services;
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
        public static ServiceCollection AddTwitchServices(this ServiceCollection services)
        {
            var configService = new ConfigService();

            var config = configService.GetConfig();

            var creds = new ConnectionCredentials(config.ChatbotNick, config.ChatbotPass);
            var client = new TwitchClient();
            client.Initialize(creds, config.StreamerChannel);

            var api = new TwitchAPI();
            api.Settings.AccessToken = config.ChatbotAccessToken;

            var liveStreamMonitor = new LiveStreamMonitorService(api);

            var pubsub = new TwitchPubSub();

            // Register all external Twitch services
            services.AddSingleton(api);
            services.AddSingleton(client);
            services.AddSingleton(pubsub);
            services.AddSingleton(liveStreamMonitor);

            return services;
        }

        public static ServiceCollection AddLibraryServices(this ServiceCollection services)
        {
            // TODO: Add a package class to library to do this.
            // Register Transient types
            services.AddTransient<IConfigService, ConfigService>();
            services.AddTransient<IChatbotContextFactory, ChatbotContextFactory>();

            return services;
        }

        public static ServiceCollection AddHelpers(this ServiceCollection services)
        {
            // Register chatbot helpers (these should really be services)
            services.AddTransient<IRedditHelper, RedditHelper>();

            return services;
        }

        public static ServiceCollection AddChatCommands(this ServiceCollection services)
        {
            services.AddTransient<ICommand, AddInfoCommand>();
            services.AddTransient<ICommand, AwwCommand>();
            services.AddTransient<ICommand, ClaimAllVipsCommand>();
            services.AddTransient<ICommand, ClaimVipCommand>();
            services.AddTransient<ICommand, ClosePlaylistCommand>();
            services.AddTransient<ICommand, DiscordCommand>();
            services.AddTransient<ICommand, EditRockRequestCommand>();
            services.AddTransient<ICommand, FollowageCommand>();
            services.AddTransient<ICommand, GiftVipCommand>();
            services.AddTransient<ICommand, GiveVipCommand>();
            services.AddTransient<ICommand, GuessCommand>();
            services.AddTransient<ICommand, HelpCommand>();
            services.AddTransient<ICommand, HowToRequestCommand>();
            services.AddTransient<ICommand, InfoCommand>();
            services.AddTransient<ICommand, InstagramCommand>();
            services.AddTransient<ICommand, KittyCommand>();
            services.AddTransient<ICommand, MerchCommand>();
            services.AddTransient<ICommand, MyBytesCommand>();
            services.AddTransient<ICommand, MyRockRequestsCommand>();
            services.AddTransient<ICommand, MyVipsCommand>();
            services.AddTransient<ICommand, OpenPlaylistCommand>();
            services.AddTransient<ICommand, PromoteCommand>();
            services.AddTransient<ICommand, RemoveCurrentRequestCommand>();
            services.AddTransient<ICommand, RemoveSuperVipCommand>();
            services.AddTransient<ICommand, RocksmithChallengeCommand>();
            services.AddTransient<ICommand, ShoutoutCommand>();
            services.AddTransient<ICommand, SocialCommand>();
            services.AddTransient<ICommand, SongListCommand>();
            services.AddTransient<ICommand, SuperVipCommand>();
            services.AddTransient<ICommand, TwitterCommand>();
            services.AddTransient<ICommand, UptimeCommand>();
            services.AddTransient<ICommand, VipCommand>();
            services.AddTransient<ICommand, YoutubeCommand>();

            // Register CommandHelper
            services.AddScoped<ICommandHelper, CommandHelper>();

            return services;
        }
    }
}