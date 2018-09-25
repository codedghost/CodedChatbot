using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private static bool isGameStarted = false;

        public GuessingGameService(IChatbotContextFactory contextFactory,
            TwitchClient client, IConfigService configService)
        {
            this.contextFactory = contextFactory;
            this.Client = client;
            this.Config = configService.GetConfig();
        }

        public void GuessingGameStart(string songName)
        {
            if (isGameStarted) return;

            isGameStarted = true;

            InitialiseGameTimer(songName);
            isGameStarted = false;
        }

        private async void InitialiseGameTimer(string songName)
        {
            if (!OpenGuessingGame(songName))
            {
                Client.SendMessage(Config.StreamerChannel, "I couldn't start the guessing game :S");
                return;
            }

            Client.SendMessage(Config.StreamerChannel,
                $"The guessing game has begun! You have 60 seconds to !guess the accuracy that {Config.StreamerChannel} will get on {songName}!");

            await Task.Delay(TimeSpan.FromSeconds(30));

            Client.SendMessage(Config.StreamerChannel, $"30 seconds until the guessing game closes!");

            await Task.Delay(TimeSpan.FromSeconds(20));

            Client.SendMessage(Config.StreamerChannel, $"10 seconds until the guessing game closes!");

            if (!CloseGuessingGame())
            {
                Client.SendMessage(Config.StreamerChannel, "I couldn't close the guessing game for some reason... SEND HALP");
            }

            Client.SendMessage(Config.StreamerChannel, "The guessing game has now closed. Good luck everyone!");
        }

        private bool OpenGuessingGame(string songName)
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

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        private bool CloseGuessingGame()
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    var currentGuessingGameRecords = context.SongGuessingRecords.Where(x => x.UsersCanGuess);

                    if (!currentGuessingGameRecords.Any())
                    {
                        Console.WriteLine("Looks like this game is already closed");
                        return false;
                    }

                    var currentGuessingGameRecord = currentGuessingGameRecords.FirstOrDefault();

                    if (currentGuessingGameRecord == null)
                    {
                        Console.WriteLine("This really shouldn't happen");
                        return false;
                    }

                    currentGuessingGameRecord.UsersCanGuess = false;
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
                    AnnounceCurrentGameWinner(currentGuessingGameRecord, context);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        private void AnnounceCurrentGameWinner(SongGuessingRecord currentGuessingRecord, IChatbotContext context)
        {
            var potentialWinnerModels = context.SongPercentageGuesses
                .Where(g => g.SongGuessingRecord.SongGuessingRecordId == currentGuessingRecord.SongGuessingRecordId).ToList()
                .Select(pg =>
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
            return context.SongGuessingRecords?.SingleOrDefault(x => x.UsersCanGuess)?.SongGuessingRecordId ?? 0;
        }

        public bool UserGuess(string username, decimal percentageGuess)
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    var currentGameId = GetRunningGameId(context);

                    if (currentGameId == 0) return false;

                    var existingGuess = context.SongPercentageGuesses.SingleOrDefault(g =>
                        g.SongGuessingRecordId == currentGameId && g.Username == username);

                    
                    if (existingGuess == null)
                    {
                        existingGuess = new SongPercentageGuess
                        {
                            Guess = percentageGuess,
                            SongGuessingRecordId = currentGameId,
                            Username = username
                        };
                        context.SongPercentageGuesses.Add(existingGuess);
                    }
                    else
                    {
                        existingGuess.Guess = percentageGuess;
                    }
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