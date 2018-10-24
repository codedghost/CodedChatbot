using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new [] {"youtube", "yt"}, false)]
    public class YoutubeCommand : ICommand
    {
        private readonly ConfigModel config;

        public YoutubeCommand(IConfigHelper configHelper)
        {
            config = configHelper.GetConfig();
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, $"Check out the Youtube channel here: {config.YoutubeLink}");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs the Youtube link from time to time.");
        }
    }
}
