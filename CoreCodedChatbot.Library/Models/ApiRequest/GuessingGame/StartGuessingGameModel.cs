using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame
{
    public class StartGuessingGameModel
    {
        public string SongName { get; set; }
        public int SongLengthSeconds { get; set; }
    }
}
