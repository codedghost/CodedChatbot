using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"instagram", "insta"}, false)]
    public class InstagramCommand : ICommand
    {
        private ConfigModel config;

        public InstagramCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"Check me out on insta: {config.InstagramLink}"
                    : $"Hey {username}, check me out on insta: {config.InstagramLink}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey {username}, this command will output {config.StreamerChannel}'s instagram link");
        }
    }
}
