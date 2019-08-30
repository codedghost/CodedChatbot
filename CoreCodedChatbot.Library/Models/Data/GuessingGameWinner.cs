using System.Collections.Generic;
using System.Linq;
using CoreCodedChatbot.Database.Context.Models;

namespace CoreCodedChatbot.Library.Models.Data
{
    public class GuessingGameWinner
    {
        public string Username { get; set; }
        public decimal Guess { get; set; }
        public decimal Difference { get; set; }
        public decimal BytesWon { get; set; }

        public static List<GuessingGameWinner> Create(List<(decimal, SongPercentageGuess pg)> potentialWinnerModels)
        {
            var orderedWinners = potentialWinnerModels.OrderBy(w => w.Item1).ToList();
            var firstWinner = orderedWinners.FirstOrDefault();
            var winners = orderedWinners.Where(w => w.Item1 == firstWinner.Item1).ToList();

            return winners.Select(winner => new GuessingGameWinner
            {
                Username = winner.Item2.Username,
                Guess = winner.Item2.Guess,
                Difference = firstWinner.Item1,
                BytesWon = (decimal) (firstWinner.Item1 == 0 ? 1.0 / winners.Count :
                    firstWinner.Item1 <= 1 ? 0.5 / winners.Count :
                    0.25 / winners.Count),
            }).OrderBy(w => w.Difference).ToList();
        }
    }
}
