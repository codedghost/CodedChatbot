using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "rocksmithchallenge", "challenge"}, false)]
    public class RocksmithChallengeCommand : ICommand
    {
        private ConfigModel config;

        public RocksmithChallengeCommand(ConfigModel config)
        {
            this.config = config;
        }


        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"Want to take part in our challenge where we learn a new song every month? Join the discord and react on the info page to get access to the challenge channel: {config.DiscordLink}"
                    : $"Hey @{username} want to take part in our challenge where we learn a new song every month? Join the discord and react on the info page to get access to the challenge channel: {config.DiscordLink}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you about the Rocksmith Challenge we run each month!");
        }
    }
}
