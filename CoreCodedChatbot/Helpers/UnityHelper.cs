using System;
using System.Threading.Tasks;
using TwitchLib;
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
            var config = ConfigHelper.GetConfig();

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

            var commandHelper = new CommandHelper(container, config);
            container.RegisterInstance(commandHelper);

            return container;
        }
    }
}
