using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "cp", "closeplaylist", "sp", "shutplaylist" }, true)]
    public class ClosePlaylistCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;
        private readonly ConfigModel config;

        public ClosePlaylistCommand(PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.playlistHelper = playlistHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, playlistHelper.ClosePlaylist() 
                ? $"Hey @{username}, I have closed the playlist!" 
                : $"Hey {username}, I can't seem to close the playlist for some reason :(");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command will close the playlist!");
        }
    }
}
