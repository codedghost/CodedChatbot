using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.ClientTrigger;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "background", "music" }, false)]
    public class BackgroundSongCommand : ICommand
    {
        private readonly IClientTriggerClient _clientTriggerClient;

        public BackgroundSongCommand(IClientTriggerClient clientTriggerClient)
        {
            _clientTriggerClient = clientTriggerClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {

            var result = await _clientTriggerClient.CheckBackgroundSong(new CheckBackgroundSongRequest
            {
                Username = username
            });

            if (!result)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, sorry but I can't do that right now. Please try again in a few minutes");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will tell you the current background song playing!");
        }
    }
}