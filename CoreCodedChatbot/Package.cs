using System;
using System.Linq;
using System.Reflection;
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
            var commands = Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                string.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) && t.IsVisible);

            foreach (var command in commands)
            {
                services.AddTransient(command);
            }

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