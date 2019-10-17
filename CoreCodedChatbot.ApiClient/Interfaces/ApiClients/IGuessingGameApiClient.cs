using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame;

namespace CoreCodedChatbot.ApiClient.Interfaces.ApiClients
{
    public interface IGuessingGameApiClient
    {
        Task<bool> StartGuessingGame(StartGuessingGameModel songInfo);
        Task<bool> FinishGuessingGame(decimal finalPercentage);
        Task<bool> SubmitGuess(SubmitGuessModel submitGuessModel);
    }
}