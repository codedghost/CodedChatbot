using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"social", "socials"}, false)]
    public class SocialCommand : ICommand
    {
        private readonly IConfigService _configService;
        private ICommandHelper _commandHelper;

        public SocialCommand(IConfigService configService, ICommandHelper commandHelper)
        {
            _configService = configService;
            this._commandHelper = commandHelper;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            await _commandHelper.ProcessCommand("discord", client, "Chatbot", string.Empty, true, joinedChannel);
            await _commandHelper.ProcessCommand("yt", client, "Chatbot", string.Empty, true, joinedChannel);
            await _commandHelper.ProcessCommand("insta", client, "Chatbot", string.Empty, true, joinedChannel);
            await _commandHelper.ProcessCommand("twitter", client, "Chatbot", string.Empty, true, joinedChannel);
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will output all of {_configService.Get<string>("StreamerChannel")}'s social media links");
        }
    }
}
