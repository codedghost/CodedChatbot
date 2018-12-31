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
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR.Client;
using TwitchLib.Api.Helix.Models.Entitlements;

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
                               .AddMinutes(2) >= DateTime.UtcNow ||
                               (request.VipRequestTime ?? DateTime.MinValue).AddMinutes(2) >=
                               DateTime.UtcNow ||
                               request.RequestTime.AddMinutes(5) >= DateTime.UtcNow,
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
                    RequestTime = DateTime.UtcNow,
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

                if (vipRequest) request.VipRequestTime = DateTime.UtcNow;

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

        public AddRequestResult AddWebRequest(RequestSongViewModel requestSongViewModel, string username)
        {
            try
            {
                if (string.IsNullOrEmpty(requestSongViewModel.Title) &&
                    string.IsNullOrWhiteSpace(requestSongViewModel.Artist)) return AddRequestResult.NoRequestEntered;

                var playlistState = GetPlaylistState();

                switch (playlistState)
                {
                    case PlaylistState.VeryClosed:
                        return AddRequestResult.PlaylistVeryClosed;
                    case PlaylistState.Closed when !requestSongViewModel.IsVip:
                        return AddRequestResult.PlaylistClosed;
                }

                using (var context = contextFactory.Create())
                {
                    if (!requestSongViewModel.IsVip)
                    {
                        var userSongCount = context.SongRequests.Count(sr =>
                            !sr.Played && sr.RequestUsername == username && sr.VipRequestTime == null);

                        if (userSongCount >= UserMaxSongCount) return AddRequestResult.NoMultipleRequests;
                    }

                    var request = new SongRequest
                    {
                        RequestTime = DateTime.UtcNow,
                        RequestText =
                            $"{requestSongViewModel.Artist} - {requestSongViewModel.Title} ({requestSongViewModel.SelectedInstrument})",
                        RequestUsername = username,
                        Played = false
                    };

                    if (requestSongViewModel.IsVip) request.VipRequestTime = DateTime.UtcNow;

                    context.SongRequests.Add(request);
                    context.SaveChanges();
                }

                UpdatePlaylists();
            }
            catch (Exception)
            {
                return AddRequestResult.UnSuccessful;
            }

            return AddRequestResult.Success;
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

                request.VipRequestTime = DateTime.UtcNow;
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

        public PlaylistViewModel GetAllSongs(LoggedInTwitchUser twitchUser = null)
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
                                       .AddMinutes(2) >= DateTime.UtcNow ||
                                       (sr.VipRequestTime ?? DateTime.MinValue).AddMinutes(5) >= DateTime.UtcNow,
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
                                       .AddMinutes(2) >= DateTime.UtcNow ||
                                       sr.RequestTime.AddMinutes(5) >= DateTime.UtcNow,
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

                return new PlaylistViewModel
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
            var currentSongs = GetAllSongs();

            currentSongs.RegularList = currentSongs.RegularList.Where(r => r.songRequestId != CurrentRequest.songRequestId)
                .ToArray();
            currentSongs.VipList = currentSongs.VipList.Where(r => r.songRequestId != CurrentRequest.songRequestId).ToArray();

            var processEditArgsResponse = ProcessEditArgs(username, commandText, currentSongs, out songRequestText);

            if (processEditArgsResponse == ProcessEditArgsResult.ArgumentError ||
                processEditArgsResponse == ProcessEditArgsResult.NoRequestInList ||
                processEditArgsResponse == ProcessEditArgsResult.NoRequestProvided)
            {
                syntaxError = true;
                return false;
            }

            UpdatePlaylists();

            syntaxError = false;
            return true;
        }

        public EditRequestResult EditWebRequest(RequestSongViewModel editRequestModel, string username, bool isMod)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(editRequestModel.Title) &&
                    string.IsNullOrWhiteSpace(editRequestModel.Artist))
                {
                    return EditRequestResult.NoRequestEntered;
                }

                using (var context = contextFactory.Create())
                {
                    var songRequest =
                        context.SongRequests.SingleOrDefault(sr => sr.SongRequestId == editRequestModel.SongRequestId);

                    if (songRequest == null) return EditRequestResult.UnSuccessful;
                    if (songRequest.Played) return EditRequestResult.RequestAlreadyRemoved;
                    if (!isMod && songRequest.RequestUsername != username) return EditRequestResult.NotYourRequest;

                    songRequest.RequestText =
                        $"{editRequestModel.Artist} - {editRequestModel.Title} ({editRequestModel.SelectedInstrument}";
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in EditWebRequest\n{e} - {e.InnerException}");
                return EditRequestResult.UnSuccessful;
            }
            
            UpdatePlaylists();

            return EditRequestResult.Success;
        }

        public PromoteRequestResult PromoteWebRequest(int songId, string username)
        {
            var vipIssued = false;
            try
            {
                using (var context = contextFactory.Create())
                {
                    var songRequest = context.SongRequests.SingleOrDefault(sr => sr.SongRequestId == songId);

                    if (songRequest == null) return PromoteRequestResult.UnSuccessful;
                    if (songRequest.RequestUsername != username) return PromoteRequestResult.NotYourRequest;
                    if (songRequest.VipRequestTime != null) return PromoteRequestResult.AlreadyVip;
                    if (!vipService.UseVip(username)) return PromoteRequestResult.NoVipAvailable;

                    vipIssued = true;
                    songRequest.VipRequestTime = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                if (vipIssued) vipService.RefundVip(username);
                Console.WriteLine($"Exception in PromoteWebRequest\n{e} - {e.InnerException}");
                return PromoteRequestResult.UnSuccessful;
            }

            UpdatePlaylists();
            return PromoteRequestResult.Successful;
        }

        public RequestSongViewModel GetNewRequestSongViewModel(string username)
        {
            return new RequestSongViewModel
            {
                ModalTitle = "Request a song",
                IsNewRequest = true,
                Title = string.Empty,
                Artist = string.Empty,
                Instruments = GetRequestInstruments(),
                SelectedInstrument = string.Empty,
                IsVip = false,
                ShouldShowVip = vipService.HasVip(username)
            };
        }

        public RequestSongViewModel GetEditRequestSongViewModel(string username, int songRequestId, bool isMod)
        {
            using (var context = contextFactory.Create())
            {
                var songRequest = context.SongRequests.SingleOrDefault(sr =>
                    !sr.Played && (sr.RequestUsername == username || isMod) && sr.SongRequestId == songRequestId);

                if (songRequest == null) return null;
                
                var formattedRequest = FormattedRequest.GetFormattedRequest(songRequest.RequestText);

                return new RequestSongViewModel
                {
                    ModalTitle = "Edit your request",
                    IsNewRequest = false,
                    SongRequestId = songRequest.SongRequestId,
                    Title = formattedRequest?.SongName ?? songRequest.RequestText,
                    Artist = formattedRequest?.SongArtist ?? string.Empty,
                    Instruments = GetRequestInstruments(formattedRequest?.InstrumentName),
                    SelectedInstrument = formattedRequest?.InstrumentName ?? "guitar",
                    IsVip = songRequest.VipRequestTime != null,
                    ShouldShowVip = false,
                };
            }
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

                    var estimatedFinishTime = DateTime.UtcNow.AddMinutes(requests * 6d).ToString("HH:mm:ss");
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

            var inChatRegularRequests = regularRequests.Where(r => r.isInChat).ToList();
            if (!inChatRegularRequests.Any() && !vipRequests.Any())
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
                else if (inChatRegularRequests.Any())
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
                else if (inChatRegularRequests.Any())
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
                    CurrentRequest = inChatRegularRequests[rand.Next(inChatRegularRequests.Count)];
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

        public int GetMaxUserRequests()
        {
            return UserMaxSongCount;
        }

        private SelectListItem[] GetRequestInstruments(string selectedInstrumentName = "guitar")
        {
            var instrumentName = string.IsNullOrWhiteSpace(selectedInstrumentName) ? "guitar" : selectedInstrumentName;
            return new []
            {
                new SelectListItem("Guitar", "guitar", instrumentName == "guitar"),
                new SelectListItem("Bass", "bass", instrumentName == "bass"), 
            };
        }

        private bool ProcessEdit(string songRequestText, PlaylistViewModel currentSongs, string username, ProcessEditArgsResult action, int songIndex = 0)
        {
            var vipRequestsWithIndex =
                currentSongs.VipList.Select((sr, index) => new { Index = index + 1, SongRequest = sr }).ToList();

            using (var context = contextFactory.Create())
            {
                PlaylistItem request;
                switch (action)
                {
                    case ProcessEditArgsResult.OneRequestEdit:
                        request = currentSongs.RegularList.SingleOrDefault(rs => rs.songRequester == username) ??
                                      currentSongs.VipList.SingleOrDefault(vs => vs.songRequester == username);

                        break;
                    case ProcessEditArgsResult.RegularRequest:
                        request = currentSongs.RegularList.SingleOrDefault(rs => rs.songRequester == username);

                        break;
                    case ProcessEditArgsResult.VipRequestNoIndex:
                        request = currentSongs.VipList.SingleOrDefault(rs => rs.songRequester == username);

                        break;
                    case ProcessEditArgsResult.VipRequestWithIndex:
                        request = vipRequestsWithIndex
                            .SingleOrDefault(rs => rs.SongRequest.songRequester == username && rs.Index == songIndex)?
                            .SongRequest;

                        break;
                    default:
                        request = null;
                        break;
                }

                if (request == null) return false;

                var dbReq = context.SongRequests.SingleOrDefault(
                    sr => sr.SongRequestId == request.songRequestId);

                if (dbReq == null) return false;

                dbReq.RequestText = songRequestText;
                context.SaveChanges();

                return true;
            }

        }

        private ProcessEditArgsResult ProcessEditArgs(string username, string commandText, PlaylistViewModel currentSongs, out string songRequestText)
        {
            var vipRequestsWithIndex =
                currentSongs.VipList.Select((sr, index) => new { Index = index + 1, SongRequest = sr }).ToList();

            songRequestText = string.Empty;

            if (currentSongs.RegularList.All(rs => rs.songRequester != username) &&
                currentSongs.VipList.All(vs => vs.songRequester != username))
            {
                return ProcessEditArgsResult.NoRequestInList;
            }

            var splitCommandText = commandText.Split(' ').ToList();
            int.TryParse(splitCommandText[0].Trim(), out var playlistIndex);
            if (playlistIndex != 0)
            {
                splitCommandText.RemoveAt(0);
            }

            songRequestText = string.Join(" ", splitCommandText);

            var totalRequestCount = (currentSongs.RegularList.Count(rs => rs.songRequester == username) +
                                    vipRequestsWithIndex.Count(vs => vs.SongRequest.songRequester == username));

            var doesUserHaveRegularRequest = currentSongs.RegularList.Any(rs => rs.songRequester == username);
            var userVips = vipRequestsWithIndex.Where(req => req.SongRequest.songRequester == username).ToList();

            if (string.IsNullOrWhiteSpace(songRequestText))
            {
                return ProcessEditArgsResult.NoRequestProvided;
            }

            if (totalRequestCount == 1)
            {
                return ProcessEdit(songRequestText, currentSongs, username, ProcessEditArgsResult.OneRequestEdit)
                    ? ProcessEditArgsResult.OneRequestEdit
                    : ProcessEditArgsResult.ArgumentError; // We can change this request regardless
            }

            if (playlistIndex != 0)
            {
                if (userVips.Count == 0)
                {
                    return ProcessEditArgsResult.ArgumentError;
                }

                return ProcessEdit(songRequestText, currentSongs, username, ProcessEditArgsResult.VipRequestWithIndex,
                    playlistIndex)
                    ? ProcessEditArgsResult.VipRequestWithIndex
                    : ProcessEditArgsResult.ArgumentError;
            }


            if (doesUserHaveRegularRequest)
            {
                return ProcessEdit(songRequestText, currentSongs, username, ProcessEditArgsResult.RegularRequest)
                    ? ProcessEditArgsResult.RegularRequest
                    : ProcessEditArgsResult.ArgumentError;
            }
            switch (userVips.Count)
            {
                case 1:
                    return ProcessEdit(songRequestText, currentSongs, username, ProcessEditArgsResult.VipRequestNoIndex)
                        ? ProcessEditArgsResult.VipRequestNoIndex
                        : ProcessEditArgsResult.ArgumentError;
                default:
                    return ProcessEditArgsResult.ArgumentError;
            }
        }
    }
}
