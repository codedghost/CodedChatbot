using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "guess" }, false)]
    public class GuessCommand : ICommand
    {
        private readonly IGuessingGameApiClient _guessingGameApiClient;
        private ConfigModel _config;

        public GuessCommand(IGuessingGameApiClient guessingGameApiClient, IConfigService configService)
        {
            _guessingGameApiClient = guessingGameApiClient;
            _config = configService.GetConfig();
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var trimPercent = commandText.Trim('%');
            if (!decimal.TryParse(trimPercent, out decimal decimalGuess))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, that doesn't look like a valid guess! Your guess needs to look like this: !guess xx.x");
                return;
            }

            var success = _guessingGameApiClient.SubmitGuess(new SubmitGuessModel
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
                $"Hey @{username}, this command lets you join the guessing game when {_config.StreamerChannel} is playing a song request!");
        }
    }
}
