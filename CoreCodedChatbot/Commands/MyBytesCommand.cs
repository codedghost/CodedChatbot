using System;
using System.Collections.Generic;
using System.Text;

using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.CustomAttributes;

using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "mybytes", "mybites" }, false)]
    public class MyBytesCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var bytes = BytesHelper.CheckBytes(username);
            client.SendMessage($"Hey @{username}, you have {bytes} Bytes!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command will tell you how many bytes you have earned by watching the stream! To convert a Byte use !claimvip");
        }
    }
}
