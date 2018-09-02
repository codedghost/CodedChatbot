using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class SongPercentageGuess
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SongPercentageGuessId { get; set; }
        public string Username { get; set; }
        public decimal Guess { get; set; }
    }
}
