using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GuessingGameApiController : Controller
    {
        private IGuessingGameService guessingGameService;

        private object timerLock = new object();

        public GuessingGameApiController(IGuessingGameService guessingGameService)
        {
            this.guessingGameService = guessingGameService;
        }

        [HttpPost]
        public IActionResult StartGuessingGame([FromBody] string songName)
        {
            try
            {
                bool isGameInProgress;
                lock (timerLock)
                {
                    // Check guessing game state
                    isGameInProgress = guessingGameService.IsGuessingGameInProgress();
                }
                
                if (!isGameInProgress) guessingGameService.GuessingGameStart(songName);

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult FinishGuessingGame([FromBody] decimal finalPercentage)
        {
            if (guessingGameService.SetPercentageAndFinishGame(finalPercentage))
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult SubmitGuess([FromBody] SubmitGuessModel submitGuessModel)
        {
            if (guessingGameService.UserGuess(submitGuessModel.Username, submitGuessModel.Guess))
                return Ok();

            return BadRequest();
        }
    }
}
