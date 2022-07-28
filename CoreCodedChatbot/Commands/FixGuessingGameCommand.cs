using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Settings;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "resetgg", "ggez" }, true)]
    public class FixGuessingGameCommand : ICommand
    {
        private readonly ISettingsApiClient _settingsApiClient;

        public FixGuessingGameCommand(ISettingsApiClient settingsApiClient)
        {
            _settingsApiClient = settingsApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var success = await _settingsApiClient.UpdateSettings(new UpdateSettingsRequest
            {
                Key = "IsGuessingGameInProgress",
                Value = "false"
            });

            client.SendMessage(joinedChannel,
                success
                    ? $"Hey @{username}, I managed to reset the Guessing Game, try it now!"
                    : $"Hey @{username}, I couldn't reset the Guessing Game, shit's fucked yo");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command lets mods fix the guessing game when it gets a bit wonky");
        }
    }
}