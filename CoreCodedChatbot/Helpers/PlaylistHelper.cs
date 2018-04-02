using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Models.Data;

namespace CoreCodedChatbot.Helpers
{
    public static class PlaylistHelper
    {
        private static ConfigModel config = ConfigHelper.GetConfig();

        private static IOrderedQueryable<SongRequest> OrderRequests(this IQueryable<SongRequest> requests)
        {
            return requests.OrderBy(sr => sr.VipRequestTime ?? DateTime.MaxValue).ThenBy(sr => sr.RequestTime);
        }

        public static int AddRequest(string username, string commandText, bool vipRequest = false)
        {
            var songIndex = 0;
            using (var context = new ChatbotContext())
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
                    var status = context.Settings
                        .SingleOrDefault(set => set.SettingName == "PlaylistStatus");
                    var playlistLength = context.SongRequests.Count(sr => !sr.Played);
                    var userSongCount = context.SongRequests.Count(sr => !sr.Played && sr.RequestUsername == username);
                    Console.Out.WriteLine($"Not a vip request: {playlistLength}, {userSongCount}");
                    if (status != null)
                    {
                        if (status?.SettingValue == null || status?.SettingValue == "Closed") return -1;
                    }
                    if (playlistLength >= 5 && userSongCount > 0)
                    {
                        Console.Out.WriteLine("returning -2");
                        return -2;
                    }
                }

                if (vipRequest) request.VipRequestTime = DateTime.Now;

                context.SongRequests.Add(request);
                context.SaveChanges();

                songIndex = context.SongRequests.Where(sr => !sr.Played).OrderRequests().ToList()
                    .FindIndex(sr => sr == request) + 1;
            }

            UpdatePlaylists();

            return songIndex;
        }

        public static int AddRequestSignalR(string username, string commandText, bool vipRequest = false)
        {
            return AddRequest(username, commandText, vipRequest);
        }

        public static int PromoteRequest(string username, int songIndex)
        {
            var newSongIndex = 0;
            using (var context = new ChatbotContext())
            {
                var request = context.SongRequests.Where(sr => !sr.Played).OrderRequests()
                    .ToList().ElementAtOrDefault(songIndex);

                if (request == null)
                    return -1; // No request at this index

                if (request.RequestUsername != username)
                    return -2; // Not this users request.

                request.VipRequestTime = DateTime.Now;
                context.SaveChanges();

                newSongIndex = context.SongRequests.Where(sr => !sr.Played).OrderRequests()
                    .ToList().FindIndex(sr => sr == request) + 1;
            }

            UpdatePlaylists();
            return newSongIndex;
        }

        private static void UpdateObsPlaylist()
        {
            using (var file = File.Open(config.ObsPlaylistPath,
                File.Exists(config.ObsPlaylistPath) ? FileMode.Truncate : FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var sw = new StreamWriter(file))
                {
                    var textToWrite = string.Empty;

                    using (var context = new ChatbotContext())
                    {
                        var requests = context.SongRequests
                            .Where(sr => !sr.Played)
                            .OrderRequests().ToList();

                        textToWrite = string.Join('\n', requests
                            .Take(5)
                            .Select((sr, index) => $"{index + 1} - {sr.RequestText} - {sr.RequestUsername}"));
                    }

                    sw.Write(textToWrite);
                }
            }
        }

        private static async void UpdateWebPlaylist()
        {
            //TODO:
            // URL needs to come from config
            var connection = new HubConnectionBuilder()
                .WithUrl($"{config.WebPlaylistUrl}")
                .WithConsoleLogger()
                .Build();

            await connection.StartAsync();

            using (var context = new ChatbotContext())
            {
                var requests = context.SongRequests
                    .Where(sr => !sr.Played)
                    .OrderRequests()
                    .Take(5)
                    .ToList()
                    .Select((sr, index) => $"{index + 1} - {sr.RequestText} - {sr.RequestUsername}")
                    .ToArray();

                await connection.InvokeAsync("Send", new[] { requests });
            }
            await connection.DisposeAsync();
        }

        private static async void UpdateFullPlaylist()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{config.WebPlaylistUrl}")
                .WithConsoleLogger()
                .Build();

            await connection.StartAsync();

            using (var context = new ChatbotContext())
            {
                var requests = context.SongRequests
                    .Where(sr => !sr.Played)
                    .OrderRequests()
                    .ToList()
                    .Select((sr, index) => $"{index + 1} - {sr.RequestText} - {sr.RequestUsername}")
                    .ToArray();

                await connection.InvokeAsync("SendAll", new[] {requests});
            }

            await connection.DisposeAsync();
        }

        public static void ArchiveCurrentRequest()
        {
            using (var context = new ChatbotContext())
            {
                var currentRequest = context.SongRequests.Where(sr => !sr.Played)
                    .OrderRequests()
                    .FirstOrDefault();

                if (currentRequest == null)
                    return;

                currentRequest.Played = true;
                context.SaveChanges();
            }

            UpdatePlaylists();
        }

        public static string GetUserRequests(string username)
        {
            var relevantItems = GetUserRelevantRequests(username);

            return relevantItems.Any() 
                    ? string.Join(", ", relevantItems) 
                    : "Looks like you don't have any songs in the queue, get requestin' dude! <!rr>";
        }

        private static List<string> GetUserRelevantRequests(string username)
        {
            using (var context = new ChatbotContext())
            {
                var userRequests = context.SongRequests
                    .Where(sr => !sr.Played)
                    ?.OrderRequests().ToList()
                    ?.Select((sr, index) => new { Index = index+1, SongRequest = sr })
                    ?.Where(x => x.SongRequest.RequestUsername == username)
                    ?.OrderBy(x => x.Index)
                    ?.Select(x => $"{x.Index} - {x.SongRequest.RequestText}")
                    ?.ToList();

                return userRequests ?? new List<string>();
            }
        }

        public static string[] GetTopSongs()
        {
            using (var context = new ChatbotContext())
            {
                var requests = context.SongRequests.Where(sr => !sr.Played)
                    .OrderRequests()
                    .Take(5)
                    .ToList()
                    .Select((sr, index) => $"{index + 1} - {sr.RequestText} - {sr.RequestUsername}")
                    .ToArray();
                return requests;
            }
        }

        public static string[] GetAllSongs()
        {
            using (var context = new ChatbotContext())
            {
                var requests = context.SongRequests.Where(sr => !sr.Played)
                    .OrderRequests()
                    .ToList()
                    .Select((sr, index) => $"{index + 1} - {sr.RequestText} - {sr.RequestUsername}")
                    .ToArray();
                return requests;
            }
        }

        public static void ClearRockRequests()
        {
            using (var context = new ChatbotContext())
            {
                var requests = context.SongRequests.Where(sr => !sr.Played);

                foreach (var request in requests)
                    request.Played = true;

                context.SaveChanges();
            }

            UpdatePlaylists();
        }

        public static bool RemoveRockRequests(string username, string commandText, bool isMod)
        {
            if (!int.TryParse(commandText.Trim(), out var playlistIndex))
            {
                return false;
            }

            using (var context = new ChatbotContext())
            {
                var userRequest = context.SongRequests
                    ?.Where(sr => !sr.Played)
                    ?.OrderRequests().ToList()
                    ?.Select((sr, index) => new { Index = index+1, SongRequest = sr })
                    ?.Where(x => (x.SongRequest.RequestUsername == username || isMod) && x.Index == playlistIndex)
                    .FirstOrDefault();

                if (userRequest == null) return false;

                context.Remove(userRequest.SongRequest);
                context.SaveChanges();
            }

            UpdatePlaylists();

            return true;
        }

        public static bool EditRequest(string username, string commandText, bool isMod, out string songRequestText, out bool syntaxError)
        {
            using (var context = new ChatbotContext())
            {
                var userRequests = context.SongRequests
                    ?.Where(sr => !sr.Played)
                    ?.OrderRequests().ToList()
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

        public static bool OpenPlaylist()
        {
            using (var context = new ChatbotContext())
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
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        public static bool ClosePlaylist()
        {
            using (var context = new ChatbotContext())
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

        private static void UpdatePlaylists()
        {
            UpdateWebPlaylist();
        }
    }
}
