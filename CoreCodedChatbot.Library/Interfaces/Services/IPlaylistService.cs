using System;
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
        PlaylistState GetPlaylistState();
        int PromoteRequest(string username);
        void UpdateFullPlaylist(bool updateCurrent = false);
        void ArchiveCurrentRequest();
        string GetUserRequests(string username);
        List<string> GetUserRelevantRequests(string username);
        PlaylistBrowserSource GetAllSongs(LoggedInTwitchUser twitchUser = null);
        void ClearRockRequests();
        bool RemoveRockRequests(string username, string commandText, bool isMod);

        bool EditRequest(string username, string commandText, bool isMod, out string songRequestText,
            out bool syntaxError);

        bool OpenPlaylist();
        bool ClosePlaylist();
        bool ArchiveRequestById(int songId);
        string GetEstimatedTime(ChatViewersModel chattersModel);
        bool VeryClosePlaylist();
    }
}
