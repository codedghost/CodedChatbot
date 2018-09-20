using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TwitchLib.Client;

namespace CoreCodedChatbot.Library.Services
{
    public class GuessingGameService : IGuessingGameService
    {
        private readonly TwitchClient Client;
        private readonly ConfigModel Config;
        private IChatbotContextFactory contextFactory;

        private Timer GameTimer;
        private int SecondsElapsed = 0;

        public GuessingGameService(IChatbotContextFactory contextFactory,
            TwitchClient client, IConfigService configService)
        {
            this.contextFactory = contextFactory;
            this.Client = client;
            this.Config = configService.GetConfig();
        }
        
        public void GuessingGameStart(string songName)
        {
            InitialiseGameTimer(songName);
        }

        private void InitialiseGameTimer(string songName)
        {
            GameTimer = new Timer(x =>
            {
                if (SecondsElapsed == 0)
                {
                    Client.SendMessage(Config.StreamerChannel,
                        $"The guessing game has begun! You have 30 seconds to !guess the accuracy that {Config.StreamerChannel} will get on {songName}!");
                    OpenGuessingGame(songName);

                    SecondsElapsed += 10;

                    return;
                }

                if (SecondsElapsed == Config.SecondsForGuessingGame)
                {
                    Client.SendMessage(Config.StreamerChannel, "The guessing game has now closed. Good luck everyone!");
                    SecondsElapsed = 0;
                    // Close guessing game in db
                    CloseGuessingGame();
                    GameTimer = null;
                    return;
                }

                Client.SendMessage(Config.StreamerChannel, $"{Config.SecondsForGuessingGame - SecondsElapsed} seconds until the guessing game closes!");
                SecondsElapsed += 10;
            }, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
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

        public bool SetPercentageAndFinishGame(decimal finalPercentage)
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
                        return false;
                    }

                    currentGuessingGameRecord.IsInProgress = false;
                    currentGuessingGameRecord.FinalPercentage = finalPercentage;
                    context.SaveChanges();

                    // Find closest guess.
                    AnnounceCurrentGameWinner(currentGuessingGameRecord);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        private void AnnounceCurrentGameWinner(SongGuessingRecord currentGuessingRecord)
        {
            var potentialWinnerModels = currentGuessingRecord.SongPercentageGuesses.Select(pg =>
                (Math.Abs(currentGuessingRecord.FinalPercentage - pg.Guess), pg));

            var orderedWinners = potentialWinnerModels.OrderBy(w => w.Item1).ToList();

            var firstWinner = orderedWinners.FirstOrDefault();
            var winners = orderedWinners.Where(w => w.Item1 == firstWinner.Item1).ToList();
            // No-one guessed?
            if (!winners.Any())
                Client.SendMessage(Config.StreamerChannel, "Nobody guessed! Good luck next time :)");

            var result = new GuessingGameWinner
            {
                Usernames = winners.Select(w => w.Item2.Username).ToArray(),
                Difference = firstWinner.Item1,
                BytesWon = (decimal)(firstWinner.Item1 == 0 ? 1 / winners.Count :
                    firstWinner.Item1 <= 1 ? 0.5 / winners.Count :
                    0.25 / winners.Count)
            };

            // TODO: URGENT -> Refactor this to own service when bytes service is brought over to library project.
            GiveBytes(result);

            Client.SendMessage(Config.StreamerChannel,
                result.Difference == 0
                    ? $"{result.FormattedUsernames} has won! You were spot on! You've received {result.BytesWon} bytes"
                    : $"{result.FormattedUsernames} has won! You were {result.Difference} away from the actual score. You've received {result.BytesWon} bytes");
        }

        private void GiveBytes(GuessingGameWinner winner)
        {
            using (var context = contextFactory.Create())
            {
                foreach (var username in winner.Usernames)
                {
                    // Find or add user
                    var user = context.Users.Find(username);

                    if (user == null)
                    {
                        user = new User
                        {
                            Username = username.ToLower(),
                            UsedVipRequests = 0,
                            ModGivenVipRequests = 0,
                            FollowVipRequest = 0,
                            SubVipRequests = 0,
                            DonationOrBitsVipRequests = 0,
                            TokenBytes = 0
                        };

                        context.Users.Add(user);
                    }

                    var bytesValueToGive = (int)Math.Round(Config.BytesToVip * winner.BytesWon);

                    user.TokenBytes += (int) Math.Round(winner.BytesWon * Config.BytesToVip);
                }

                context.SaveChanges();
            }
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

        public bool UserGuess(string username, decimal percentageGuess)
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    var currentGameId = GetRunningGameId(context);

                    if (currentGameId == 0) return false;

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