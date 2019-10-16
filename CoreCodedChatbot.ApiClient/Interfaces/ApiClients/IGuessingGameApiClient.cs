using CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame;

namespace CoreCodedChatbot.ApiClient.Interfaces.ApiClients
{
    public interface IGuessingGameApiClient
    {
        bool StartGuessingGame(StartGuessingGameModel songInfo);
        bool FinishGuessingGame(decimal finalPercentage);
        bool SubmitGuess(SubmitGuessModel submitGuessModel);
    }
}