﻿using System;
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

        private string FormatRequest(SongRequest sr, int index) => $"{index + 1} - {sr.RequestText} - {sr.RequestUsername}";

        private string FormatRequestNoIndex(SongRequest sr) =>
            $"{PrefixVip(sr)} - {sr.RequestText} - {sr.RequestUsername}";

        private string PrefixVip(SongRequest request) => request.VipRequestTime.HasValue ? " (VIP)" : string.Empty;


        public PlaylistService(IChatbotContextFactory contextFactory, IConfigService configService)
        {
            this.contextFactory = contextFactory;
            this.config = configService.GetConfig();
        }

        public PlaylistItem GetRequestById(int songId)
        {
            using (var context = contextFactory.Create())
            {
                var request = context.SongRequests.Find(songId);
                return new PlaylistItem
                {
                    songRequestId = request.SongRequestId,
                    songRequestText = FormatRequestNoIndex(request),
                    isInChat = (context.Users.SingleOrDefault(u => u.Username == request.RequestUsername)
                                    ?.TimeLastInChat ?? DateTime.MinValue)
                               .ToUniversalTime()
                               .AddMinutes(2) >= DateTime.UtcNow ||
                               (request.VipRequestTime ?? DateTime.MinValue).ToUniversalTime().AddMinutes(2) >=
                               DateTime.UtcNow ||
                               request.RequestTime.ToUniversalTime().AddMinutes(5) >= DateTime.UtcNow
                };
            }
        }

        public (AddRequestResult, int) AddRequest(string username, string commandText, bool vipRequest = false)
        {
            var songIndex = 0;
            var isPlaylistOpen = IsPlaylistOpen();
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
                    if (!isPlaylistOpen)
                    {
                        return (AddRequestResult.PlaylistClosed, 0);
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
            }

            UpdatePlaylists();

            return (AddRequestResult.Success, songIndex);
        }

        public bool IsPlaylistOpen()
        {
            using (var context = contextFactory.Create())
            {
                var status = context.Settings
                    .SingleOrDefault(set => set.SettingName == "PlaylistStatus");
                return status?.SettingValue != null && status.SettingValue != "Closed";
            }
        }

        public int PromoteRequest(string username, int songIndex)
        {
            var newSongIndex = 0;
            using (var context = contextFactory.Create())
            {
                var request = context.SongRequests.Where(sr => !sr.Played).OrderRequests()
                    .ElementAtOrDefault(songIndex);

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

        public async void UpdateFullPlaylist()
        {
            var psk = config.SignalRKey;

            var connection = new HubConnectionBuilder()
                .WithUrl($"{config.WebPlaylistUrl}/SongList")
                .Build();

            await connection.StartAsync();

            var requests = GetAllSongs();

            await connection.InvokeAsync<SongListHubModel>("SendAll",
                new SongListHubModel
                {
                    psk = psk,
                    regularRequests = requests.RegularList,
                    vipRequests = requests.VipList
                });

            await connection.DisposeAsync();
        }

        public void ArchiveCurrentRequest()
        {
            using (var context = contextFactory.Create())
            {
                var currentRequest = context.SongRequests.Where(sr => !sr.Played)
                    .OrderRequests()
                    .FirstOrDefault();

                if (currentRequest == null)
                    return;

                Console.Out.WriteLine(currentRequest.RequestText);

                currentRequest.Played = true;
                context.SaveChanges();
            }

            UpdatePlaylists();
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
                    ?.Select((sr, index) => new {Index = index + 1, SongRequest = sr})
                    ?.Where(x => x.SongRequest.RequestUsername == username)
                    ?.OrderBy(x => x.Index)
                    ?.Select(x => $"{x.Index} - {x.SongRequest.RequestText}")
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
                            songRequestText = FormatRequest(sr, index),
                            isInChat = (context.Users.SingleOrDefault(u => u.Username == sr.RequestUsername)?.TimeLastInChat ?? DateTime.MinValue)
                                       .ToUniversalTime()
                                       .AddMinutes(2) >= DateTime.UtcNow ||
                                       (sr.VipRequestTime ?? DateTime.MinValue).ToUniversalTime().AddMinutes(5) >= DateTime.UtcNow
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
                            songRequestText = FormatRequest(sr, index + vipRequests.Length),
                            isInChat = (context.Users.SingleOrDefault(u => u.Username == sr.RequestUsername)
                                            ?.TimeLastInChat ?? DateTime.MinValue)
                                       .ToUniversalTime()
                                       .AddMinutes(2) >= DateTime.UtcNow ||
                                       sr.RequestTime.ToUniversalTime().AddMinutes(5) >= DateTime.UtcNow
                        };
                    }).ToArray();

                return new PlaylistBrowserSource
                {
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
                return false;
            }

            using (var context = contextFactory.Create())
            {
                var userRequest = context.SongRequests
                    ?.Where(sr => !sr.Played)
                    ?.OrderRequests()
                    ?.Select((sr, index) => new { Index = index + 1, SongRequest = sr })
                    ?.Where(x => (x.SongRequest.RequestUsername == username || isMod) && x.Index == playlistIndex)
                    .FirstOrDefault();

                if (userRequest == null) return false;

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

                var userRequestCount = userRequests?.Where(x => x.SongRequest.RequestUsername == username).Count();
                if (userRequestCount != 1 && playlistIndex == 0)
                {
                    songRequestText = string.Empty;
                    syntaxError = true;
                    return false;
                }
                else if (userRequestCount > 1 && playlistIndex == 0)
                {
                    songRequestText = string.Empty;
                    syntaxError = true;
                    return false;
                }

                songRequestText = string.Join(" ", splitCommandText);

                if (string.IsNullOrWhiteSpace(songRequestText))
                {
                    syntaxError = true;
                    return false;
                }


                if (isMod)
                {
                    var userRequest = userRequests?.Where(x => x.Index == playlistIndex).FirstOrDefault();

                    if (userRequest == null)
                    {
                        syntaxError = true;
                        return false;
                    }

                    userRequest.SongRequest.RequestText = songRequestText;

                    context.SongRequests.Update(userRequest.SongRequest);
                    context.SaveChanges();
                }
                else if (userRequestCount == 1)
                {
                    if (playlistIndex != 0)
                    {
                        var userRequest = userRequests?.Where(x => x.SongRequest.RequestUsername == username && x.Index == playlistIndex).FirstOrDefault();
                        if (userRequest == null)
                        {
                            songRequestText = string.Empty;
                            syntaxError = false;
                            return false;
                        }

                        userRequest.SongRequest.RequestText = songRequestText;

                        context.SongRequests.Update(userRequest.SongRequest);
                        context.SaveChanges();
                    }
                    else
                    {
                        var userRequest = userRequests?.Where(x => x.SongRequest.RequestUsername == username).FirstOrDefault();

                        userRequest.SongRequest.RequestText = songRequestText;

                        context.SongRequests.Update(userRequest.SongRequest);
                        context.SaveChanges();
                    }
                }
                else
                {
                    var userRequest = userRequests.FirstOrDefault(x => x.SongRequest.RequestUsername == username && x.Index == playlistIndex);

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

        private void UpdatePlaylists()
        {
            UpdateFullPlaylist();
        }
    }
}
