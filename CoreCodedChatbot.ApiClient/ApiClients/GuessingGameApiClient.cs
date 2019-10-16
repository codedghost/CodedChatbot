using CoreCodedChatbot.Library.Interfaces.ApiClients;

namespace CoreCodedChatbot.ApiClient.ApiClients
{
    public class GuessingGameApiClient : IGuessingGameApiClient
    {
        public bool StartGuessingGame(StartGuessingGameModel songInfo)
        {
            throw new System.NotImplementedException();
        }

        public bool FinishGuessingGame(decimal finalPercentage)
        {
            throw new System.NotImplementedException();
        }

        public bool SubmitGuess(SubmitGuessModel submitGuessModel)
        {
            throw new System.NotImplementedException();
        }
    }
}