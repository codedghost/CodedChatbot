using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Models.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TwitchLib.Client;

namespace CoreCodedChatbot.Library.Services
{
    public class GuessingGameService
    {
        private readonly TwitchClient Client;
        private readonly ConfigModel Config;
        private IChatbotContextFactory contextFactory;

        private Timer GameTimer;
        private int SecondsElapsed = 0;

        public GuessingGameService(IChatbotContextFactory contextFactory,
            TwitchClient client, ConfigService configService)
        {
            this.contextFactory = contextFactory;
            this.Client = client;
            this.Config = configService.GetConfig();
        }
        
        public void GuessingGameStart(string songName)
        {
            Client.SendMessage(Config.StreamerChannel,
                $"The guessing game has begun! You have 30 seconds to !guess the accuracy that {Config.StreamerChannel} will get on {songName}!");

            InitialiseGameTimer(songName);
        }

        private void InitialiseGameTimer(string songName)
        {
            GameTimer = new Timer(x =>
            {
                if (SecondsElapsed == 0)
                {
                    OpenGuessingGame(songName);
                }

                SecondsElapsed += 10;

                if (SecondsElapsed == Config.SecondsForGuessingGame)
                {
                    Client.SendMessage(Config.StreamerChannel, "The guessing game has now closed. Good luck everyone!");
                    SecondsElapsed = 0;
                    // Close guessing game in db
                    CloseGuessingGame();
                    GameTimer = null;
                    return;
                }

                Client.SendMessage(Config.StreamerChannel, $"{Config.SecondsForGuessingGame - SecondsElapsed} until the guessing game closes!");
            }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        }

        private void OpenGuessingGame(string songName)
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    CloseExistingGames(context);

                    var newGuessRecord = new SongGuessingRecord
                    {
                        SongDetails = songName,
                        UsersCanGuess = true,
                        IsInProgress = true
                    };

                    context.SongGuessingRecords.Add(newGuessRecord);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
            }
        }

        private void CloseGuessingGame()
        {
            using (var context = contextFactory.Create())
            {
                var currentGuessingGameRecord = context.SongGuessingRecords.SingleOrDefault(x => x.UsersCanGuess);

                if (currentGuessingGameRecord == null)
                {
                    Console.WriteLine("Looks like this game is already closed");
                    return;
                }

                currentGuessingGameRecord.UsersCanGuess = false;
                context.SaveChanges();
            }
        }

        public GuessingGameWinner SetPercentageAndFinishGame(decimal finalPercentage)
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    var currentGuessingGameRecord = context.SongGuessingRecords.SingleOrDefault(x => x.IsInProgress);

                    if (currentGuessingGameRecord == null)
                    {
                        Console.WriteLine(
                            "Either the game has already been completed or there is currently more than 1 guessing game running");
                        return null;
                    }

                    currentGuessingGameRecord.IsInProgress = false;
                    currentGuessingGameRecord.FinalPercentage = finalPercentage;
                    context.SaveChanges();

                    // Find closest guess.
                    return GetCurrentGameWinner(currentGuessingGameRecord);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return null;
            }
        }

        private GuessingGameWinner GetCurrentGameWinner(SongGuessingRecord currentGuessingRecord)
        {
            var potentialWinnerModels = currentGuessingRecord.SongPercentageGuesses.Select(pg =>
                (Math.Abs(currentGuessingRecord.FinalPercentage - pg.Guess), pg));

            var winner = potentialWinnerModels.OrderBy(w => w.Item1).FirstOrDefault();

            // No-one guessed?
            if (winner.Item1 == 0 && winner.Item2 == null) return new GuessingGameWinner();

            return new GuessingGameWinner
            {
                Username = winner.Item2.Username,
                Difference = winner.Item1,
                BytesWon = winner.Item1 == 0 ? Config.BytesToVip : winner.Item1 <= 1 ? Config.BytesToVip / 2 : Config.BytesToVip / 4
            };
        }

        private int GetRunningGameId()
        {
            using (var context = contextFactory.Create())
            {
                return GetRunningGameId(context);
            }
        }

        private int GetRunningGameId(IChatbotContext context)
        {
            return context.SongGuessingRecords?.SingleOrDefault(x => x.IsInProgress)?.SongGuessingRecordId ?? 0;
        }

        public bool UserGuess(decimal percentageGuess, string username)
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    var currentGameId = GetRunningGameId(context);

                    var newGuess = new SongPercentageGuess
                    {
                        Guess = percentageGuess,
                        SongGuessingRecordId = currentGameId,
                        Username = username
                    };

                    context.SongPercentageGuesses.Add(newGuess);
                    context.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        private void CloseExistingGames(IChatbotContext context)
        {
            // Unlikely, but ensure that other open games are closed

            var unclosedGames = context.SongGuessingRecords.Where(x => x.UsersCanGuess || x.IsInProgress);

            foreach (var unclosedGame in unclosedGames)
            {
                unclosedGame.UsersCanGuess = false;
                unclosedGame.IsInProgress = false;
            }

            context.SaveChanges();
        }
    }
}