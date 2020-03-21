using CoreCodedChatbot.Config;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
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

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            _commandHelper.ProcessCommand("discord", client, "Chatbot", string.Empty, true, joinedChannel);
            _commandHelper.ProcessCommand("yt", client, "Chatbot", string.Empty, true, joinedChannel);
            _commandHelper.ProcessCommand("insta", client, "Chatbot", string.Empty, true, joinedChannel);
            _commandHelper.ProcessCommand("twitter", client, "Chatbot", string.Empty, true, joinedChannel);
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will output all of {_configService.Get<string>("StreamerChannel")}'s social media links");
        }
    }
}
