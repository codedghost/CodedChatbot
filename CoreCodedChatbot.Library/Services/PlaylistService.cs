using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Extensions;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using CoreCodedChatbot.Library.Models.SignalR;
using CoreCodedChatbot.Library.Models.View;
using Microsoft.AspNetCore.SignalR.Client;

namespace CoreCodedChatbot.Library.Services
{
    public class PlaylistService : IPlaylistService
    {
        private const int UserMaxSongCount = 1;

        private readonly ConfigModel config;
        private readonly IChatbotContextFactory contextFactory;
        private readonly IVipService vipService;

        private PlaylistItem CurrentRequest;
        private int CurrentVipRequestsPlayed;
        private int ConcurrentVipSongsToPlay;
        private Random rand = new Random();

        public PlaylistService(IChatbotContextFactory contextFactory, IConfigService configService, IVipService vipService)
        {
            this.contextFactory = contextFactory;
            this.config = configService.GetConfig();
            this.vipService = vipService;

            this.ConcurrentVipSongsToPlay = config.ConcurrentRegularSongsToPlay;
        }

        public PlaylistItem GetRequestById(int songId)
        {
            using (var context = contextFactory.Create())
            {
                var request = context.SongRequests.Find(songId);
                return new PlaylistItem
                {
                    songRequestId = request.SongRequestId,
                    songRequestText = request.RequestText,
                    songRequester = request.RequestUsername,
                    isInChat = (context.Users.SingleOrDefault(u => u.Username == request.RequestUsername)
                                    ?.TimeLastInChat ?? DateTime.MinValue)
                               .ToUniversalTime()
                               .AddMinutes(2) >= DateTime.UtcNow ||
                               (request.VipRequestTime ?? DateTime.MinValue).ToUniversalTime().AddMinutes(2) >=
                               DateTime.UtcNow ||
                               request.RequestTime.ToUniversalTime().AddMinutes(5) >= DateTime.UtcNow,
                    isVip = request.VipRequestTime != null
                };
            }
        }

        public (AddRequestResult, int) AddRequest(string username, string commandText, bool vipRequest = false)
        {
            var songIndex = 0;
            var playlistState = this.GetPlaylistState();
            using (var context = contextFactory.Create())
            {
                var request = new SongRequest
                {
                    RequestTime = DateTime.Now,
                    RequestText = commandText,
                    RequestUsername = username,
                    Played = false
                };

                if (!vipRequest)
                {
                    var playlistLength = context.SongRequests.Count(sr => !sr.Played);
                    var userSongCount = context.SongRequests.Count(sr => !sr.Played && sr.RequestUsername == username && sr.VipRequestTime == null);
                    Console.Out.WriteLine($"Not a vip request: {playlistLength}, {userSongCount}");
                    if (playlistState == PlaylistState.Closed)
                    {
                        return (AddRequestResult.PlaylistClosed, 0);
                    } else if (playlistState == PlaylistState.VeryClosed)
                    {
                        return (AddRequestResult.PlaylistVeryClosed, 0);
                    }

                    if (userSongCount >= UserMaxSongCount)
                    {
                        return (AddRequestResult.NoMultipleRequests, 0);
                    }
                }

                if (vipRequest) request.VipRequestTime = DateTime.Now;

                context.SongRequests.Add(request);
                context.SaveChanges();

                songIndex = context.SongRequests.Where(sr => !sr.Played).OrderRequests()
                                .FindIndex(sr => sr == request) + 1;

                if (CurrentRequest == null)
                {
                    CurrentRequest = new PlaylistItem
                    {
                        songRequestId = request.SongRequestId,
                        songRequestText = request.RequestText,
                        songRequester = request.RequestUsername,
                        isEvenIndex = false,
                        isInChat = true,
                        isVip = vipRequest
                    };
                }
            }

            UpdatePlaylists();

            return (AddRequestResult.Success, songIndex);
        }

        public PlaylistState GetPlaylistState()
        {
            using (var context = contextFactory.Create())
            {
                var status = context.Settings
                    .SingleOrDefault(set => set.SettingName == "PlaylistStatus");
                if (status?.SettingValue == null)
                {
                    return PlaylistState.VeryClosed;
                }

                return Enum.Parse<PlaylistState>(status?.SettingValue);
            }
        }

        public int PromoteRequest(string username)
        {
            var newSongIndex = 0;
            using (var context = contextFactory.Create())
            {
                var request = context.SongRequests.FirstOrDefault(sr => !sr.Played && sr.VipRequestTime == null && sr.RequestUsername == username);

                if (request == null)
                    return -1; // No request at this index

                if (request.RequestUsername != username)
                    return -2; // Not this users request.

                request.VipRequestTime = DateTime.Now;
                context.SaveChanges();

                newSongIndex = context.SongRequests.Where(sr => !sr.Played).OrderRequests()
                                   .FindIndex(sr => sr == request) + 1;
            }

            UpdatePlaylists();
            return newSongIndex;
        }

        public async void UpdateFullPlaylist(bool updateCurrent = false)
        {
            var psk = config.SignalRKey;

            var connection = new HubConnectionBuilder()
                .WithUrl($"{config.WebPlaylistUrl}/SongList")
                .Build();

            await connection.StartAsync();

            var requests = GetAllSongs();

            if (updateCurrent)
            {
                UpdateCurrentSong(requests.RegularList, requests.VipList);
            }

            requests.RegularList = requests.RegularList.Where(r => r.songRequestId != CurrentRequest.songRequestId)
                .ToArray();
            requests.VipList = requests.VipList.Where(r => r.songRequestId != CurrentRequest.songRequestId).ToArray();

            await connection.InvokeAsync<SongListHubModel>("SendAll",
                new SongListHubModel
                {
                    psk = psk,
                    currentSong = CurrentRequest,
                    regularRequests = requests.RegularList,
                    vipRequests = requests.VipList
                });

            await connection.DisposeAsync();
        }

        public void ArchiveCurrentRequest(int songId = 0)
        {
            // SongId of zero indicates that the command has been called from twitch chat

            using (var context = contextFactory.Create())
            {
                var currentRequest = songId == 0 ? CurrentRequest :
                    songId == CurrentRequest.songRequestId ? CurrentRequest : null;

                if (currentRequest == null)
                    return;

                var currentRequestDbModel = context.SongRequests.Find(currentRequest.songRequestId);

                if (currentRequestDbModel == null)
                    return;

                Console.Out.WriteLine(currentRequestDbModel.RequestText);

                currentRequestDbModel.Played = true;
                context.SaveChanges();
            }

            UpdatePlaylists(true);
        }

        public string GetUserRequests(string username)
        {
            var relevantItems = GetUserRelevantRequests(username);

            return relevantItems.Any()
                ? string.Join(", ", relevantItems)
                : "Looks like you don't have any songs in the queue, get requestin' dude! <!rr>";
        }

        public List<string> GetUserRelevantRequests(string username)
        {
            using (var context = contextFactory.Create())
            {
                var userRequests = context.SongRequests
                    .Where(sr => !sr.Played)
                    ?.OrderRequests()
                    ?.Select((sr, index) => new { Index = index + 1, SongRequest = sr })
                    ?.Where(x => x.SongRequest.RequestUsername == username)
                    ?.OrderBy(x => x.Index)
                    ?.Select(x =>
                        x.SongRequest.VipRequestTime != null
                            ? $"{x.Index} - {x.SongRequest.RequestText}"
                            : x.SongRequest.RequestText)
                    ?.ToList();

                return userRequests ?? new List<string>();
            }
        }

        public PlaylistBrowserSource GetAllSongs(LoggedInTwitchUser twitchUser = null)
        {
            using (var context = contextFactory.Create())
            {
                var vipRequests = context.SongRequests.Where(sr => !sr.Played && sr.VipRequestTime != null)
                    .OrderRequests()
                    .Select((sr, index) =>
                    {
                        return new PlaylistItem
                        {
                            songRequestId = sr.SongRequestId,
                            songRequestText = sr.RequestText,
                            songRequester = sr.RequestUsername,
                            isInChat = (context.Users.SingleOrDefault(u => u.Username == sr.RequestUsername)?.TimeLastInChat ?? DateTime.MinValue)
                                       .ToUniversalTime()
                                       .AddMinutes(2) >= DateTime.UtcNow ||
                                       (sr.VipRequestTime ?? DateTime.MinValue).ToUniversalTime().AddMinutes(5) >= DateTime.UtcNow,
                            isVip = sr.VipRequestTime != null,
                            isEvenIndex = index % 2 == 0
                        };
                    })
                    .ToArray();

                var regularRequests = context.SongRequests.Where(sr => !sr.Played && sr.VipRequestTime == null)
                    .OrderRequests()
                    .Select((sr, index) =>
                    {
                        return new PlaylistItem
                        {
                            songRequestId = sr.SongRequestId,
                            songRequestText = sr.RequestText,
                            songRequester = sr.RequestUsername,
                            isInChat = (context.Users.SingleOrDefault(u => u.Username == sr.RequestUsername)
                                            ?.TimeLastInChat ?? DateTime.MinValue)
                                       .ToUniversalTime()
                                       .AddMinutes(2) >= DateTime.UtcNow ||
                                       sr.RequestTime.ToUniversalTime().AddMinutes(5) >= DateTime.UtcNow,
                            isVip = sr.VipRequestTime != null,
                            isEvenIndex = index % 2 == 0
                        };
                    }).ToArray();

                // Ensure if the playlist is populated then a request is made current
                if (CurrentRequest == null)
                {
                    if (vipRequests.Any())
                    {
                        CurrentRequest = vipRequests.First();
                        vipRequests = vipRequests.Where(r => r.songRequestId != CurrentRequest.songRequestId).ToArray();
                    } else if (regularRequests.Any())
                    {
                        CurrentRequest = regularRequests[rand.Next(0, regularRequests.Length)];
                        regularRequests = regularRequests.Where(r => r.songRequestId != CurrentRequest.songRequestId).ToArray();
                    }
                }

                return new PlaylistBrowserSource
                {
                    CurrentSong = CurrentRequest,
                    RegularList = regularRequests,
                    VipList = vipRequests,
                    TwitchUser = twitchUser
                };
            }
        }

        public void ClearRockRequests()
        {
            using (var context = contextFactory.Create())
            {
                var requests = context.SongRequests.Where(sr => !sr.Played);

                foreach (var request in requests)
                    request.Played = true;

                context.SaveChanges();
            }

            UpdatePlaylists();
        }

        public bool RemoveRockRequests(string username, string commandText, bool isMod)
        {
            if (!int.TryParse(commandText.Trim(), out var playlistIndex))
            {
                // Try and find regular request
                using (var context = contextFactory.Create())
                {
                    var userRequest = context.SongRequests
                        ?.FirstOrDefault(
                            sr => !sr.Played && sr.VipRequestTime == null && sr.RequestUsername == username);
                    if (userRequest == null) return false;

                    context.Remove(userRequest);
                    context.SaveChanges();
                    UpdatePlaylists();
                    return true;
                }
            }

            using (var context = contextFactory.Create())
            {
                // We have a playlist number remove VIP
                var userRequest = context.SongRequests
                    ?.Where(sr => !sr.Played && sr.VipRequestTime != null)
                    ?.OrderRequests()
                    ?.Select((sr, index) => new { Index = index + 1, SongRequest = sr })
                    ?.Where(x => (x.SongRequest.RequestUsername == username || isMod) && x.Index == playlistIndex)
                    .FirstOrDefault();

                if (userRequest == null) return false;

                vipService.RefundVip(userRequest.SongRequest.RequestUsername);

                context.Remove(userRequest.SongRequest);
                context.SaveChanges();
            }

            UpdatePlaylists();

            return true;
        }

        public bool EditRequest(string username, string commandText, bool isMod, out string songRequestText, out bool syntaxError)
        {
            using (var context = contextFactory.Create())
            {
                var userRequests = context.SongRequests
                    ?.Where(sr => !sr.Played)
                    ?.OrderRequests()
                    ?.Select((sr, index) => new { Index = index + 1, SongRequest = sr });

                if (userRequests == null)
                {
                    songRequestText = string.Empty;
                    syntaxError = false;
                    return false;
                }

                var splitCommandText = commandText.Split(' ').ToList();
                int.TryParse(splitCommandText[0].Trim(), out var playlistIndex);
                if (playlistIndex != 0)
                {
                    splitCommandText.RemoveAt(0);
                }

                var userRequestCount = userRequests.Count(x => x.SongRequest.RequestUsername == username);
                var isUserRegularRequestPreset = userRequests.Any(x =>
                    x.SongRequest.RequestUsername == username && x.SongRequest.VipRequestTime == null);
                var userVipRequestCount = userRequests.Count(x =>
                    x.SongRequest.RequestUsername == username && x.SongRequest.VipRequestTime != null);

                songRequestText = string.Join(" ", splitCommandText);

                if (string.IsNullOrWhiteSpace(songRequestText))
                {
                    syntaxError = true;
                    return false;
                }

                if (userRequestCount != 1 && playlistIndex == 0)
                {
                    if (isUserRegularRequestPreset)
                    {
                        // edit regular request
                        var userRequest = userRequests.SingleOrDefault(x =>
                            x.SongRequest.RequestUsername == username && x.SongRequest.VipRequestTime == null);
                        if (userRequest == null)
                        {
                            songRequestText = string.Empty;
                            syntaxError = false;
                            return false;
                        }


                        userRequest.SongRequest.RequestText = songRequestText;

                        context.SongRequests.Update(userRequest.SongRequest);
                        context.SaveChanges();

                        UpdatePlaylists();

                        syntaxError = false;
                        return true;
                    } else if (userVipRequestCount > 1 || userVipRequestCount == 0)
                    {
                        songRequestText = string.Empty;
                        syntaxError = true;
                        return false;
                    } else
                    {
                        // edit only vip request
                        var userRequest = userRequests.FirstOrDefault(x => x.SongRequest.RequestUsername == username && x.SongRequest.VipRequestTime != null);
                        if (userRequest == null)
                        {
                            songRequestText = string.Empty;
                            syntaxError = true;
                            return false;
                        }

                        userRequest.SongRequest.RequestText = songRequestText;

                        context.SongRequests.Update(userRequest.SongRequest);
                        context.SaveChanges();

                        UpdatePlaylists();

                        syntaxError = false;
                        return true;
                    }
                } else if (userRequestCount > 1 && userVipRequestCount > 1 && playlistIndex == 0)
                {
                    songRequestText = string.Empty;
                    syntaxError = true;
                    return false;
                }


                if (isMod)
                {
                    var userRequest = userRequests.FirstOrDefault(x => x.Index == playlistIndex);

                    if (userRequest == null)
                    {
                        syntaxError = true;
                        return false;
                    }

                    userRequest.SongRequest.RequestText = songRequestText;

                    context.SongRequests.Update(userRequest.SongRequest);
                    context.SaveChanges();
                } else if (userRequestCount == 1)
                {
                    if (playlistIndex != 0)
                    {
                        if (userVipRequestCount > 0)
                        {
                            var userRequest = userRequests?.Where(x => x.SongRequest.RequestUsername == username && x.Index == playlistIndex).FirstOrDefault();


                            userRequest.SongRequest.RequestText = songRequestText;

                            context.SongRequests.Update(userRequest.SongRequest);
                            context.SaveChanges();

                            if (userRequest != null)
                            {
                                songRequestText = string.Empty;
                                syntaxError = false;
                                return false;
                            }
                        } else
                        {
                            songRequestText = string.Empty;
                            syntaxError = false;
                            return false;
                        }
                    } else
                    {
                        var userRequest = userRequests?.Where(x => x.SongRequest.RequestUsername == username).FirstOrDefault();

                        userRequest.SongRequest.RequestText = songRequestText;

                        context.SongRequests.Update(userRequest.SongRequest);
                        context.SaveChanges();
                    }
                } else
                {
                    // Vip edit
                    var userRequest = userRequests.FirstOrDefault(x => x.SongRequest.RequestUsername == username && x.Index == playlistIndex);
                    if (userRequest == null)
                    {
                        syntaxError = false;
                        return false;
                    }

                    userRequest.SongRequest.RequestText = songRequestText;

                    context.SongRequests.Update(userRequest.SongRequest);
                    context.SaveChanges();
                }
            }

            UpdatePlaylists();

            syntaxError = false;
            return true;
        }

        public bool OpenPlaylist()
        {
            using (var context = contextFactory.Create())
            {
                try
                {
                    var playlistStatusSetting = context.Settings
                        ?.SingleOrDefault(set => set.SettingName == "PlaylistStatus");

                    if (playlistStatusSetting == null)
                    {
                        playlistStatusSetting = new Setting
                        {
                            SettingName = "PlaylistStatus",
                            SettingValue = "Open"
                        };

                        context.Settings.Add(playlistStatusSetting);
                        context.SaveChanges();
                        return true;
                    }

                    playlistStatusSetting.SettingValue = "Open";
                    context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool ClosePlaylist()
        {
            using (var context = contextFactory.Create())
            {
                try
                {
                    var playlistStatusSetting = context.Settings
                        ?.SingleOrDefault(set => set.SettingName == "PlaylistStatus");

                    if (playlistStatusSetting == null)
                    {
                        playlistStatusSetting = new Setting
                        {
                            SettingName = "PlaylistStatus",
                            SettingValue = "Closed"
                        };

                        context.Settings.Add(playlistStatusSetting);
                        context.SaveChanges();
                        return true;
                    }

                    playlistStatusSetting.SettingValue = "Closed";
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.ToString());
                    return false;
                }
            }
        }

        public bool ArchiveRequestById(int songId)
        {
            using (var context = contextFactory.Create())
            {
                var request = context.SongRequests.Find(songId);



                if (request == null) return false;

                request.Played = true;

                if (request.VipRequestTime != null)
                {
                    vipService.RefundVip(request.RequestUsername);
                }

                context.SaveChanges();

                UpdatePlaylists();

                return true;
            }
        }

        public string GetEstimatedTime(ChatViewersModel chattersModel)
        {
            using (var context = contextFactory.Create())
            {
                try
                {
                    var allViewers = chattersModel.chatters.viewers
                        .Concat(chattersModel.chatters.admins)
                        .Concat(chattersModel.chatters.global_mods)
                        .Concat(chattersModel.chatters.moderators)
                        .Concat(chattersModel.chatters.staff)
                        .ToArray();

                    var requests = context.SongRequests.Where(sr => !sr.Played)
                        .OrderRequests()
                        .Count(sr => allViewers.Contains(sr.RequestUsername));

                    var estimatedFinishTime = DateTime.Now.AddMinutes(requests * 6d).ToString("HH:mm:ss");
                    return $"Estimated time to finish: {estimatedFinishTime}";
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e);
                    return string.Empty;
                }
            }
        }

        private void UpdateCurrentSong(PlaylistItem[] regularRequests, PlaylistItem[] vipRequests)
        {
            RequestTypes updateDecision;

            if (!regularRequests.Any() && !vipRequests.Any())
            {
                CurrentRequest = null;
                return;
            }

            if (CurrentRequest.isVip)
            {
                CurrentVipRequestsPlayed++;
                if (CurrentVipRequestsPlayed < ConcurrentVipSongsToPlay
                    && vipRequests.Any())
                {
                    updateDecision = RequestTypes.Vip;
                }
                else if (regularRequests.Any())
                {
                    CurrentVipRequestsPlayed = 0;
                    updateDecision = RequestTypes.Regular;
                }
                else if (vipRequests.Any())
                {
                    updateDecision = RequestTypes.Vip;
                }
                else
                {
                    updateDecision = RequestTypes.Empty;
                }
            }
            else
            {
                if (vipRequests.Any())
                {
                    updateDecision = RequestTypes.Vip;
                }
                else if (regularRequests.Any())
                {
                    updateDecision = RequestTypes.Regular;
                }
                else
                {
                    updateDecision = RequestTypes.Empty;
                }
            }

            switch (updateDecision)
            {
                case RequestTypes.Regular:
                    CurrentRequest = regularRequests[rand.Next(0, regularRequests.Length)];
                    break;
                case RequestTypes.Vip:
                    CurrentRequest = vipRequests.FirstOrDefault();
                    break;
                case RequestTypes.Empty:
                    CurrentRequest = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdatePlaylists(bool updateCurrent = false)
        {
            UpdateFullPlaylist(updateCurrent);
        }

        public bool VeryClosePlaylist()
        {
            using (var context = contextFactory.Create())
            {
                try
                {
                    var playlistStatusSetting = context.Settings
                        ?.SingleOrDefault(set => set.SettingName == "PlaylistStatus");

                    if (playlistStatusSetting == null)
                    {
                        playlistStatusSetting = new Setting
                        {
                            SettingName = "PlaylistStatus",
                            SettingValue = "VeryClosed"
                        };

                        context.Settings.Add(playlistStatusSetting);
                        context.SaveChanges();
                        return true;
                    }

                    playlistStatusSetting.SettingValue = "VeryClosed";
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine(e.ToString());
                    return false;
                }
            }
        }
    }
}
