using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Services;

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
            var client = new TwitchClient(creds, config.StreamerChannel);
            var api = new TwitchAPI(accessToken: config.ChatbotAccessToken);
            var followerService = new FollowerService(api, 5);
            var pubsub = new TwitchPubSub();

            container.RegisterInstance(api);
            container.RegisterInstance(client);
            container.RegisterInstance(followerService);
            container.RegisterInstance(pubsub);

            var commandHelper = new CommandHelper(container);
            container.RegisterInstance(commandHelper);

            return container;
        }
    }
}
