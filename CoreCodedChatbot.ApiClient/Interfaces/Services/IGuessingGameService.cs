namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IGuessingGameService
    {
        void GuessingGameStart(string songName, int songLengthInSeconds);
        bool SetPercentageAndFinishGame(decimal finalPercentage);
        bool UserGuess(string username, decimal percentageGuess);
        bool IsGuessingGameInProgress();
        bool SetGuessingGameState(bool state);
    }
}
