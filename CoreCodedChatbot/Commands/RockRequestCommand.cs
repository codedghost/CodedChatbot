using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client;


namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "request", "rr", "sr", "songrequest", "rockrequest", "song" }, false)]
    public class RockRequestCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly ConfigModel config;

        public RockRequestCommand(PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.playlistHelper = playlistHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(config.StreamerChannel, $"Hi @{username}, looks like you haven't included a request there!");
                return;
            }

            var (result, playlistPosition) = playlistHelper.AddRequest(username, commandText);
            if (result == AddRequestResult.PlaylistClosed)
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, the playlist is currently closed. If you want to request a song still, try !vip");
            }
            else if (result == AddRequestResult.NoMultipleRequests)
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, you can only have one non-vip request in the list!");
            }
            else
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, I have queued {commandText} for you, you're #{playlistPosition} in the queue!");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, this command is used to add a song request to the queue. Usage: !request <SongArtist> - <SongName>");
        }
    }
}
