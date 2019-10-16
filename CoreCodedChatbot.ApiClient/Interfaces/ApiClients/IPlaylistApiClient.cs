using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Enums;

namespace CoreCodedChatbot.ApiClient.Interfaces.ApiClients
{
    public interface IPlaylistApiClient
    {
        EditRequestResponse EditRequest(EditSongRequest editSongRequest);
        GetUserRequestsResponse GetUserRequests(string username);
        bool OpenPlaylist();
        bool VeryClosePlaylist();
        bool ClosePlaylist();
        PlaylistState IsPlaylistOpen();
        bool ArchiveCurrentRequest();
        bool RemoveRockRequests(RemoveSongRequest removeSongRequest);
        bool RemoveSuperVip(RemoveSuperVipRequest removeSuperVipRequest);
        AddRequestResponse AddSong(AddSongRequest addSongRequest);
        AddRequestResponse AddSuperVip(AddSuperVipRequest addSuperVipRequest);
        EditRequestResponse EditSuperVip(EditSuperVipRequest editSuperVipRequest);
        int PromoteSong(PromoteSongRequest promoteSongRequest);
        bool ClearRequests();
    }
}