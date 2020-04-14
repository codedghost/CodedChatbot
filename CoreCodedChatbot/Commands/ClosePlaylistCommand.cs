using System;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "cp", "closeplaylist", "sp", "shutplaylist" }, true)]
    public class ClosePlaylistCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public ClosePlaylistCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            bool response;
            if (commandText.Equals("very", StringComparison.OrdinalIgnoreCase))
            {
                response = await _playlistApiClient.VeryClosePlaylist();
            } 
            else
            {
                response = await _playlistApiClient.ClosePlaylist();
            }

            client.SendMessage(joinedChannel, response
                ? $"Hey @{username}, I have closed the playlist{(commandText.Equals("very", StringComparison.OrdinalIgnoreCase) ? " completely" : string.Empty)}!"
                : $"Hey {username}, I can't seem to close the playlist for some reason :(");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will close the playlist!");
        }
    }
}
