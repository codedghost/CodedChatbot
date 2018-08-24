using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "rcr", "removecurrentrequest" }, true)]
    public class RemoveCurrentRequestCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly ConfigModel config;

        public RemoveCurrentRequestCommand(PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.playlistHelper = playlistHelper;
            this.config = config;
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
