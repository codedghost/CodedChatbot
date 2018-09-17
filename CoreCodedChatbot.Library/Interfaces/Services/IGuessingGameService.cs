﻿using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IGuessingGameService
    {
        void GuessingGameStart(string songName);
        GuessingGameWinner SetPercentageAndFinishGame(decimal finalPercentage);
        bool UserGuess(decimal percentageGuess, string username);
    }
}
