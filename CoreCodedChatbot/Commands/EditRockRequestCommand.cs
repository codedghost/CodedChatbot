using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "editrequest", "err", "editrockrequest", "editsong", "edit" }, false)]
    public class EditRockRequestCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;
        private readonly ConfigModel config;

        public EditRockRequestCommand(PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.playlistHelper = playlistHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            string songRequestText = string.Empty;
            bool syntaxError = false;
            var success = playlistHelper.EditRequest(username, commandText, isMod, out songRequestText, out syntaxError);

            if (success)
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username} I have successfully changed your request to: {songRequestText}");
            }
            else
            {
                client.SendMessage(config.StreamerChannel,
                    syntaxError
                        ? $"Hey @{username} command usage: !err <SongNumber> <NewSongRequest>"
                        : $"Hey @{username} it doesn't look like that's your request");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, use this command to edit your request. Use !myrequests to check your SongRequestIndex. Usage: !editrequest <Optional SongNumber> <NewSongRequest>");
        }
    }
}
