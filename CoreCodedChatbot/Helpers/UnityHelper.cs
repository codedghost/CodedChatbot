using System;
using System.Linq;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Helpers.Interfaces;

using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

using Unity;

namespace CoreCodedChatbot.Helpers
{
    public static class UnityHelper
    {
        public static IUnityContainer Create()
        {
            var container = new UnityContainer();
            container.RegisterType<IConfigHelper, ConfigHelper>();
            var configHelper = container.Resolve<IConfigHelper>();
            var config = configHelper.GetConfig();

            var creds = new ConnectionCredentials(config.ChatbotNick, config.ChatbotPass);
            var client = new TwitchClient();
            client.Initialize(creds, config.StreamerChannel);

            var api = new TwitchAPI();
            api.Settings.AccessToken = config.ChatbotAccessToken;

            var liveStreamMonitor = new LiveStreamMonitorService(api);

            var pubsub = new TwitchPubSub();

            container.RegisterInstance(api);
            container.RegisterInstance(client);
            container.RegisterInstance(pubsub);
            container.RegisterInstance(liveStreamMonitor);
            container.RegisterInstance(config);

            container.RegisterType<IChatbotContextFactory, ChatbotContextFactory>();

            var chatbotContextFactory = container.Resolve<IChatbotContextFactory>();

            var commandHelper = new CommandHelper(container, config, chatbotContextFactory);
            container.RegisterInstance(commandHelper);

            return container;
        }
    }
}
