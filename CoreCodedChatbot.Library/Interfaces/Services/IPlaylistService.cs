﻿using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.Enums;
using CoreCodedChatbot.Library.Models.View;

namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IPlaylistService
    {
        PlaylistItem GetRequestById(int songId);
        (AddRequestResult, int) AddRequest(string username, string commandText, bool vipRequest = false);
        AddRequestResult AddWebRequest(RequestSongViewModel requestSongViewModel, string username);
        PlaylistState GetPlaylistState();
        int PromoteRequest(string username);
        void UpdateFullPlaylist(bool updateCurrent = false);
        void ArchiveCurrentRequest(int songId = 0);
        string GetUserRequests(string username);
        List<string> GetUserRelevantRequests(string username);
        PlaylistViewModel GetAllSongs(LoggedInTwitchUser twitchUser = null);
        void ClearRockRequests();
        bool RemoveRockRequests(string username, string commandText, bool isMod);

        bool EditRequest(string username, string commandText, bool isMod, out string songRequestText,
            out bool syntaxError);

        EditRequestResult EditWebRequest(RequestSongViewModel editRequestModel, string username, bool isMod);
        PromoteRequestResult PromoteWebRequest(int songId, string username);

        bool AddSongToDrive(int songId);

        RequestSongViewModel GetNewRequestSongViewModel(string username);
        RequestSongViewModel GetEditRequestSongViewModel(string username, int songRequestId, bool isMod);

        bool OpenPlaylist();
        bool ClosePlaylist();
        bool ArchiveRequestById(int songId);
        string GetEstimatedTime(ChatViewersModel chattersModel);
        bool VeryClosePlaylist();
        int GetMaxUserRequests();
    }
}
