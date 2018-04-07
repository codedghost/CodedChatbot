using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using TwitchLib;



namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "request", "rr", "sr", "songrequest", "rockrequest", "song" }, false)]
    public class RockRequestCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        public RockRequestCommand(PlaylistHelper playlistHelper)
        {
            this.playlistHelper = playlistHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage($"Hi @{username}, looks like you haven't included a request there!");
                return;
            }

            var playlistPosition = playlistHelper.AddRequest(username, commandText);
            if (playlistPosition == -1)
            {
                client.SendMessage($"Hey @{username}, the playlist is currently closed. If you want to request a song still, try !vip");
            }
            else if (playlistPosition == -2)
            {
                client.SendMessage($"Hey @{username}, the playlist has more than five songs, so you can only have one non-vip request in the list right now!");
            }
            else
            {
                client.SendMessage($"Hey @{username}, I have queued {commandText} for you, you're #{playlistPosition} in the queue!");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(
                $"Hey @{username}, this command is used to add a song request to the queue. Usage: !request <SongArtist> - <SongName>");
        }
    }
}
