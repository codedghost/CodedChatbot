using System.Net.Sockets;
using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{"clearrequests", "clearrockrequests" }, true)]
    public class ClearRockRequestsCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly ConfigModel config;

        public ClearRockRequestsCommand(PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.playlistHelper = playlistHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            playlistHelper.ClearRockRequests();

            client.SendMessage(config.StreamerChannel, $"@{username} Hey, I've cleared all requests for you!");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will clear all requests from the queue.");
        }
    }
}
