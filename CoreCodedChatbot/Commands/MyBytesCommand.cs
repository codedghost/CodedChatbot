﻿using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "mybytes", "mybites" }, false)]
    public class MyBytesCommand : ICommand
    {
        private readonly BytesHelper bytesHelper;

        private readonly ConfigModel config;

        public MyBytesCommand(BytesHelper bytesHelper, ConfigModel config)
        {
            this.bytesHelper = bytesHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var bytes = bytesHelper.CheckBytes(username);
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, you have {bytes} Bytes!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will tell you how many bytes you have earned by watching the stream! To convert a Byte use !claimvip");
        }
    }
}
