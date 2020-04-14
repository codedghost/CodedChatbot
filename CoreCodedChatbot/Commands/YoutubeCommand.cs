using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new [] {"youtube", "yt"}, false)]
    public class YoutubeCommand : ICommand
    {
        private readonly IConfigService _configService;

        public YoutubeCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Check out the Youtube channel here: {_configService.Get<string>("YoutubeLink")}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs the Youtube link from time to time.");
        }
    }
}
