using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "rocksmithchallenge", "challenge"}, false)]
    public class RocksmithChallengeCommand : ICommand
    {
        private readonly IConfigService _configService;

        public RocksmithChallengeCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"Want to take part in our challenge where we learn a new song every month? Join the discord and react on the info page to get access to the challenge channel: {_configService.Get<string>("DiscordLink")}"
                    : $"Hey @{username} want to take part in our challenge where we learn a new song every month? Join the discord and react on the info page to get access to the challenge channel: {_configService.Get<string>("DiscordLink")}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you about the Rocksmith Challenge we run each month!");
        }
    }
}
