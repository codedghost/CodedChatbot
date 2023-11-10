using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "twitter" }, false)]
    public class TwitterCommand : ICommand
    {
        private readonly IConfigService _configService;

        public TwitterCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Follow me on twitter too: {_configService.Get<string>("TwitterLink")}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs the twitter link from time to time.");
        }
    }
}
