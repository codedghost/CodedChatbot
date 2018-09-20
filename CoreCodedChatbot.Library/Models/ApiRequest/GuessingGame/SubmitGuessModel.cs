using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame
{
    public class SubmitGuessModel
    {
        public string Username { get; set; }
        public decimal Guess { get; set; }
    }
}
