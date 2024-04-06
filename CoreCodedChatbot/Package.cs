using CodedChatbot.TwitchFactories;
using CoreCodedChatbot.Commands;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Services;
using Microsoft.Extensions.DependencyInjection;
using TwitchLib.PubSub;

namespace CoreCodedChatbot
{
    public static class Package
    {
        public static IServiceCollection AddTwitchServices(this IServiceCollection services)
        {
            services.AddTwitchFactories();

            var pubsub = new TwitchPubSub();

            services.AddSingleton(pubsub);

            return services;
        }

        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            // Register chatbot helpers (these should really be services)
            services.AddTransient<IRedditHelper, RedditHelper>();
            services.AddSingleton<IHelpHelper, HelpHelper>();

            return services;
        }

        public static IServiceCollection AddChatCommands(this IServiceCollection services)
        {
            services.AddTransient<AddInfoCommand>();
            services.AddTransient<AddQuoteCommand>();
            services.AddTransient<AwwCommand>();
            services.AddTransient<BackgroundSongCommand>();
            services.AddTransient<ClaimAllVipsCommand>();
            services.AddTransient<ClaimVipCommand>();
            services.AddTransient<ClosePlaylistCommand>();
            services.AddTransient<CounterCommand>();
            services.AddTransient<CurrentSongCommand>();
            services.AddTransient<DiceCommand>();
            services.AddTransient<DiscordCommand>();
            services.AddTransient<EditRockRequestCommand>();
            services.AddTransient<EditQuoteCommand>();
            services.AddTransient<FixGuessingGameCommand>();
            services.AddTransient<FollowageCommand>();
            services.AddTransient<GetQuoteCommand>();
            services.AddTransient<GiftedVipsCommand>();
            services.AddTransient<GiftVipCommand>();
            services.AddTransient<GiveVipCommand>();
            services.AddTransient<GuessCommand>();
            services.AddTransient<HelpCommand>();
            services.AddTransient<HowToRequestCommand>();
            services.AddTransient<InfoCommand>();
            services.AddTransient<InstagramCommand>();
            services.AddTransient<KittyCommand>();
            services.AddTransient<Magic8BallCommand>();
            services.AddTransient<MerchCommand>();
            services.AddTransient<MyBytesCommand>();
            services.AddTransient<MyRockRequestsCommand>();
            services.AddTransient<MyVipsCommand>();
            services.AddTransient<OpenPlaylistCommand>();
            services.AddTransient<PromoteCommand>();
            services.AddTransient<RemoveCurrentRequestCommand>();
            services.AddTransient<RemoveQuoteCommand>();
            services.AddTransient<RemoveRockRequestCommand>();
            services.AddTransient<RemoveSuperVipCommand>();
            services.AddTransient<ResetCounterCommand>();
            services.AddTransient<RockRequestCommand>();
            services.AddTransient<RocksmithChallengeCommand>();
            services.AddTransient<ShoutoutCommand>();
            services.AddTransient<SocialCommand>();
            services.AddTransient<SongListCommand>();
            services.AddTransient<SuperVipCommand>();
            services.AddTransient<TopTenCommand>();
            services.AddTransient<TwitterCommand>();
            services.AddTransient<UpdateCounterSuffixCommand>();
            services.AddTransient<UptimeCommand>();
            services.AddTransient<VipCommand>();
            services.AddTransient<WatchTimeCommand>();
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