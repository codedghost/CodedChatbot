using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"rocksmith", "rs"}, false)]
    public class RocksmithCommand : ICommand
    {
        private ConfigModel config;

        public RocksmithCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"This is Rocksmith 2014 Remastered Edition! Check it out here: {config.RocksmithLink}"
                    : $"Hey @{username}, this is Rocksmith 2014 Remastered Edition! Check it out here: {config.RocksmithLink}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you about Rocksmith!");
        }
    }
}
