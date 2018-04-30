﻿using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Helpers.Interfaces;

using TwitchLib.Api;
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
            api.InitializeAsync(accessToken: config.ChatbotAccessToken).Wait();

            var pubsub = new TwitchPubSub();

            container.RegisterInstance(api);
            container.RegisterInstance(client);
            container.RegisterInstance(pubsub);
            container.RegisterInstance(config);

            container.RegisterType<IChatbotContextFactory, ChatbotContextFactory>();
            var contextFactory = new ChatbotContextFactory();
            container.RegisterInstance(contextFactory);

            var commandHelper = new CommandHelper(container, config);
            container.RegisterInstance(commandHelper);

            return container;
        }
    }
}
