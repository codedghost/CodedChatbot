using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.GuessingGame;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "guess" }, false)]
    public class GuessCommand : ICommand
    {
        private readonly IGuessingGameApiClient _guessingGameApiClient;
        private readonly IConfigService _configService;

        public GuessCommand(IGuessingGameApiClient guessingGameApiClient, IConfigService configService)
        {
            _guessingGameApiClient = guessingGameApiClient;
            _configService = configService;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var trimPercent = commandText.Trim('%');
            if (!decimal.TryParse(trimPercent, out decimal decimalGuess))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, that doesn't look like a valid guess! Your guess needs to look like this: !guess xx.x");
                return;
            }

            var success = await _guessingGameApiClient.SubmitGuess(new SubmitGuessRequest
            {
                Username = username,
                Guess = decimalGuess
            });

            if (success)
            {
                client.SendMessage(joinedChannel, $"@{username} has guessed {trimPercent}. Good luck!");
                return;
            }

            client.SendMessage(joinedChannel,
                $"Sorry @{username}, the guessing game isn't open yet! Looks like you might have missed your chance for this song?");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command lets you join the guessing game when {_configService.Get<string>("StreamerChannel")} is playing a song request!");
        }
    }
}
