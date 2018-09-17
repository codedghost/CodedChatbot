using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.Data
{
    public class GuessingGameWinner
    {
        public string Username { get; set; }
        public decimal Difference { get; set; }
        public decimal BytesWon { get; set; }
    }
}
