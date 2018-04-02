using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "op", "openplaylist" }, true)]
    public class OpenPlaylistCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(PlaylistHelper.OpenPlaylist() 
                ? $"Hey @{username}, I have opened the playlist for you" 
                : $"Hey @{username}, I can't seem to open the playlist :(");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage($"Hey @{username}, this command will open the playlist!");
        }
    }
}
