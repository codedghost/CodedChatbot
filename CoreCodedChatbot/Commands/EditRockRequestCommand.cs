using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Helpers;
using TwitchLib;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "editrequest", "err", "editrockrequest", "editsong", "edit" }, false)]
    public class EditRockRequestCommand : ICommand
    {
        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            string songRequestText = string.Empty;
            bool syntaxError = false;
            var success = PlaylistHelper.EditRequest(username, commandText, isMod, out songRequestText, out syntaxError);

            if (success)
            {
                client.SendMessage($"Hey @{username} I have successfully changed your request to: {songRequestText}");
            }
            else
            {
                client.SendMessage(
                    syntaxError
                        ? $"Hey @{username} command usage: !err <SongNumber> <NewSongRequest>"
                        : $"Hey @{username} it doesn't look like that's your request");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(
                $"Hey @{username}, use this command to edit your request. Use !myrequests to check your SongRequestIndex. Usage: !editrequest <Optional SongNumber> <NewSongRequest>");
        }
    }
}
