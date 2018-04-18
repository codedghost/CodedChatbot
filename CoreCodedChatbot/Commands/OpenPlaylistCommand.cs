using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "op", "openplaylist" }, true)]
    public class OpenPlaylistCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly ConfigModel config;

        public OpenPlaylistCommand(PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.playlistHelper = playlistHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, playlistHelper.OpenPlaylist() 
                ? $"Hey @{username}, I have opened the playlist for you" 
                : $"Hey @{username}, I can't seem to open the playlist :(");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will open the playlist!");
        }
    }
}
