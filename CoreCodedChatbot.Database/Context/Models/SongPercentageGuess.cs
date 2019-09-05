using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class SongPercentageGuess
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SongPercentageGuessId { get; set; }
        public int SongGuessingRecordId { get; set; }
        public string Username { get; set; }
        public decimal Guess { get; set; }

        public virtual SongGuessingRecord SongGuessingRecord { get; set; }
    }
}
