using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

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

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Follow me on twitter too: {_configService.Get<string>("TwitterLink")}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs the twitter link from time to time.");
        }
    }
}
