using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "followme"}, true)]
    public class FollowMeCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage($"Hope you like what you're hearing! Give that Follow button a quick click ;)");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command gently reminds everyone to follow the channel!");
        }
    }
}
