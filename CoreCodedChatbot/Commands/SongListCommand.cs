using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "songlist", "playlist", "requests", "list", "songs" }, true)]
    public class SongListCommand: ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage($"Hey @{username}, the full playlist can be found at: http://localhost:49420/playlist");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command outputs a link to the current playlist");
        }
    }
}
