using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "merch" }, false)]
    public class MerchCommand : ICommand
    {
        private readonly IConfigService _configService;

        public MerchCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"Check out {_configService.Get<string>("StreamerChannel")}'s merch over at: {_configService.Get<string>("MerchLink")}"
                    : $"Hey @{username}, check out {_configService.Get<string>("StreamerChannel")}'s merch over at: {_configService.Get<string>("MerchLink")}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will output a link to {_configService.Get<string>("StreamerChannel")}'s merch page from time to time");
        }
    }
}
