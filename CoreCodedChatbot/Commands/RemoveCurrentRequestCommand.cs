using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "rcr", "removecurrentrequest" }, true)]
    public class RemoveCurrentRequestCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly ConfigModel config = ConfigHelper.GetConfig();

        public RemoveCurrentRequestCommand(PlaylistHelper playlistHelper)
        {
            this.playlistHelper = playlistHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            playlistHelper.ArchiveCurrentRequest();

            client.SendMessage(config.StreamerChannel, $"Hey @{username}, the current request has been removed");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will remove the current request from the queue.");
        }
    }
}
