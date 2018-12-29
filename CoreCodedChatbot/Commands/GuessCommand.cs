using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "guess" }, false)]
    public class GuessCommand : ICommand
    {
        private ConfigModel Config;
        private HttpClient GuessingGameClient;

        public GuessCommand(ConfigModel config)
        {
            this.Config = config;
            
            this.GuessingGameClient = new HttpClient
            {
                BaseAddress = new Uri(Config.GuessingGameApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", Config.JwtTokenString)
                }
            };
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var trimPercent = commandText.Trim('%');
            if (!decimal.TryParse(trimPercent, out decimal decimalGuess))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, that doesn't look like a valid guess! Your guess needs to look like this: !guess xx.x");
                return;
            }

            var response = await GuessingGameClient.PostAsync("SubmitGuess", HttpClientHelper.GetJsonData(new { Username = username, Guess = decimalGuess}));
            if (response.IsSuccessStatusCode)
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
                $"Hey @{username}, this command lets you join the guessing game when {Config.StreamerChannel} is playing a song request!");
        }
    }
}
