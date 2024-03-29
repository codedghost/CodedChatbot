﻿using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "songlist", "playlist", "requests", "list", "songs", "sl" }, false)]
    public class SongListCommand: ICommand
    {
        private readonly IConfigService _configService;

        public SongListCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"The full playlist can be found at: {_configService.Get<string>("WebPlaylistUrl")}/stream/playlist You can now request/edit/remove requests over there too!"
                    : $"Hey @{username}, the full playlist can be found at: {_configService.Get<string>("WebPlaylistUrl")}/stream/playlist You can now request/edit/remove requests over there too!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs a link to the current playlist");
        }
    }
}
