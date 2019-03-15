using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "reverse" }, false)]
    public class ReverseCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, some streamers have their strings reversed (or inverse) in Rocksmith because some people find it much easier to read this way." +
                $" It is very similar to the way that guitar tabs are read and can be a lot easier for some. Hope this helps!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will explain why some streamers have their strings upside down in Rocksmith!");
        }
    }
}
