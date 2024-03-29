﻿using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"instagram", "insta"}, false)]
    public class InstagramCommand : ICommand
    {
        private readonly IConfigService _configService;

        public InstagramCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"Check me out on insta: {_configService.Get<string>("InstagramLink")}"
                    : $"Hey {username}, check me out on insta: {_configService.Get<string>("InstagramLink")}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey {username}, this command will output {_configService.Get<string>("StreamerChannel")}'s instagram link");
        }
    }
}
