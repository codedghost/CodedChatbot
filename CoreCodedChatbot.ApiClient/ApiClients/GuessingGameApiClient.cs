using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame;
using Newtonsoft.Json;

namespace CoreCodedChatbot.ApiClient.ApiClients
{
    public class GuessingGameApiClient : IGuessingGameApiClient
    {
        private HttpClient _guessingGameClient;

        public GuessingGameApiClient(IConfigService configService)
        {
            var config = configService.GetConfig();
            _guessingGameClient = new HttpClient
            {
                BaseAddress = new Uri(config.GuessingGameApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
        }

        public async Task<bool> StartGuessingGame(StartGuessingGameModel songInfo)
        {
            var result = await _guessingGameClient.PostAsync("StartGuessingGame",
                HttpClientHelper.GetJsonData(songInfo));

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> FinishGuessingGame(decimal finalPercentage)
        {
            var result = await _guessingGameClient.PostAsync("FinishGuessingGame",
                HttpClientHelper.GetJsonData(finalPercentage));

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> SubmitGuess(SubmitGuessModel submitGuessModel)
        {
            var result = await _guessingGameClient.PostAsync("SubmitGuess",
                HttpClientHelper.GetJsonData(submitGuessModel));

            return result.IsSuccessStatusCode;
        }
    }
}