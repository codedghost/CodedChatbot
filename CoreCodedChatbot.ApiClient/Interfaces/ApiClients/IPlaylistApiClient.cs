using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Enums;

namespace CoreCodedChatbot.ApiClient.Interfaces.ApiClients
{
    public interface IPlaylistApiClient
    {
        Task<EditRequestResponse> EditRequest(EditSongRequest editSongRequest);
        Task<GetUserRequestsResponse> GetUserRequests(string username);
        Task<bool> OpenPlaylist();
        Task<bool> VeryClosePlaylist();
        Task<bool> ClosePlaylist();
        Task<PlaylistState> IsPlaylistOpen();
        Task<bool> ArchiveCurrentRequest();
        Task<bool> RemoveRockRequests(RemoveSongRequest removeSongRequest);
        Task<bool> RemoveSuperVip(RemoveSuperVipRequest removeSuperVipRequest);
        Task<AddRequestResponse> AddSong(AddSongRequest addSongRequest);
        Task<AddRequestResponse> AddSuperVip(AddSuperVipRequest addSuperVipRequest);
        Task<EditRequestResponse> EditSuperVip(EditSuperVipRequest editSuperVipRequest);
        Task<int> PromoteSong(PromoteSongRequest promoteSongRequest);
        Task<bool> ClearRequests();
    }
}