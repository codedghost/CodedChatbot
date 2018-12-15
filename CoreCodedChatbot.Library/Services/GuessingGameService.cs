﻿using System;
using System.Collections.Concurrent;
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
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Library.Services
{
    public class GuessingGameService : IGuessingGameService
    {
        private readonly TwitchClient Client;
        private readonly TwitchAPI Api;
        private readonly ConfigModel Config;
        private IChatbotContextFactory contextFactory;

        private JoinedChannel JoinedChannel;

        private const string GuessingGameStateSettingKey = "IsGuessingGameInProgress";
        
        public GuessingGameService(IChatbotContextFactory contextFactory,
            TwitchClient client, TwitchAPI api, IConfigService configService)
        {
            this.contextFactory = contextFactory;
            this.Client = client;
            this.Api = api;
            this.Config = configService.GetConfig();

            if (Config.developmentBuild)
            {
                Api.V5.Chat.GetChatRoomsByChannelAsync(Config.ChannelId, Config.ChatbotAccessToken)
                    .ContinueWith(
                        rooms =>
                        {
                            if (!rooms.IsCompletedSuccessfully) return;
                            var devRoomId = rooms.Result.Rooms.SingleOrDefault(r => r.Name == "dev")?.Id;
                            JoinedChannel = Client.JoinedChannels.FirstOrDefault(jc => jc.Channel.Contains(devRoomId));
                        });
            }
        }

        public void GuessingGameStart(string songName, int songLengthInSeconds)
        {
            InitialiseGameTimer(songName, songLengthInSeconds);
        }

        private async void InitialiseGameTimer(string songName, int songLengthInSeconds)
        {
            if (songLengthInSeconds < Config.SecondsForGuessingGame)
            {
                return;
            }

            if (!OpenGuessingGame(songName))
            {
                
                Client.SendMessage(JoinedChannel ?? Client.GetJoinedChannel(Config.StreamerChannel), "I couldn't start the guessing game :S");
                return;
            }

            SetGuessingGameState(true);

            Client.SendMessage(JoinedChannel ?? Client.GetJoinedChannel(Config.StreamerChannel),
                $"The guessing game has begun! You have {Config.SecondsForGuessingGame} seconds to !guess the accuracy that {Config.StreamerChannel} will get on {songName}!");

            await Task.Delay(TimeSpan.FromSeconds(Config.SecondsForGuessingGame));

            if (!CloseGuessingGame())
            {
                Client.SendMessage(JoinedChannel ?? Client.GetJoinedChannel(Config.StreamerChannel), "I couldn't close the guessing game for some reason... SEND HALP");
            }

            Client.SendMessage(JoinedChannel ?? Client.GetJoinedChannel(Config.StreamerChannel), "The guessing game has now closed. Good luck everyone!");

            SetGuessingGameState(false);
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
                (Math.Floor(Math.Abs(currentGuessingRecord.FinalPercentage - pg.Guess) * 10) / 10, pg));

            var orderedWinners = potentialWinnerModels.OrderBy(w => w.Item1).ToList();

            var firstWinner = orderedWinners.FirstOrDefault();
            var winners = orderedWinners.Where(w => w.Item1 == firstWinner.Item1).ToList();
            // No-one guessed?
            if (!winners.Any())
                Client.SendMessage(JoinedChannel ?? Client.GetJoinedChannel(Config.StreamerChannel), "Nobody guessed! Good luck next time :)");

            var result = new GuessingGameWinner
            {
                Usernames = winners.Select(w => w.Item2.Username).ToArray(),
                Difference = firstWinner.Item1,
                BytesWon = (decimal)(firstWinner.Item1 == 0 ? 1.0 / winners.Count :
                    firstWinner.Item1 <= 1 ? 0.5 / winners.Count :
                    0.25 / winners.Count)
            };

            // TODO: URGENT -> Refactor this to own service when bytes service is brought over to library project.
            GiveBytes(result);

            Client.SendMessage(JoinedChannel ?? Client.GetJoinedChannel(Config.StreamerChannel),
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
        
        public bool IsGuessingGameInProgress()
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    var guessingGameVal =
                        context.Settings.SingleOrDefault(s => s.SettingName == GuessingGameStateSettingKey);

                    if (guessingGameVal == null)
                    {
                        guessingGameVal = new Setting
                        {
                            SettingName = GuessingGameStateSettingKey,
                            SettingValue = "false"
                        };

                        context.Settings.Add(guessingGameVal);
                        context.SaveChanges();
                    }

                    return guessingGameVal.SettingValue == "true";
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"{e} - {e.InnerException}");
                return true;
            }
        }

        public bool SetGuessingGameState(bool state)
        {
            try
            {
                using (var context = contextFactory.Create())
                {
                    var currentState =
                        context.Settings.SingleOrDefault(s => s.SettingName == GuessingGameStateSettingKey);

                    if (currentState == null)
                    {
                        currentState = new Setting
                        {
                            SettingName = GuessingGameStateSettingKey
                        };

                        context.Settings.Add(currentState);
                    }

                    currentState.SettingValue = state ? "true" : "false";
                    context.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }
    }
}