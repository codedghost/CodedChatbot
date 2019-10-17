using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.Playlist;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Enums;
using Newtonsoft.Json;

namespace CoreCodedChatbot.ApiClient.ApiClients
{
    public class PlaylistApiClient : IPlaylistApiClient
    {
        private HttpClient _playlistClient;

        public PlaylistApiClient(IConfigService configService)
        {
            var config = configService.GetConfig();
            _playlistClient = new HttpClient
            {
                BaseAddress = new Uri(config.PlaylistApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
        }

        public async Task<EditRequestResponse> EditRequest(EditSongRequest editSongRequest)
        {
            var result = await _playlistClient.PostAsync("EditRequest",
                HttpClientHelper.GetJsonData(editSongRequest));

            return JsonConvert.DeserializeObject<EditRequestResponse>(await result.Content.ReadAsStringAsync());
        }

        public async Task<GetUserRequestsResponse> GetUserRequests(string username)
        {
            var result = await _playlistClient.PostAsync("GetUserRequests",
                HttpClientHelper.GetJsonData(username));

            return JsonConvert.DeserializeObject<GetUserRequestsResponse>(await result.Content.ReadAsStringAsync());
        }

        public async Task<bool> OpenPlaylist()
        {
            var result = await _playlistClient.GetAsync("OpenPlaylist");

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> VeryClosePlaylist()
        {
            var result = await _playlistClient.GetAsync("VeryClosePlaylist");

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> ClosePlaylist()
        {
            var result = await _playlistClient.GetAsync("ClosePlaylist");

            return result.IsSuccessStatusCode;
        }

        public async Task<PlaylistState> IsPlaylistOpen()
        {
            var result = await _playlistClient.GetAsync("IsPlaylistOpen");

            return JsonConvert.DeserializeObject<PlaylistState>(await result.Content.ReadAsStringAsync());
        }

        public async Task<bool> ArchiveCurrentRequest()
        {
            var result = await _playlistClient.GetAsync("ArchiveCurrentRequest");

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveRockRequests(RemoveSongRequest removeSongRequest)
        {
            var result = await _playlistClient.PostAsync("RemoveRockRequests",
                HttpClientHelper.GetJsonData(removeSongRequest));

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveSuperVip(RemoveSuperVipRequest removeSuperVipRequest)
        {
            var result = await _playlistClient.PostAsync("RemoveSuperVip",
                HttpClientHelper.GetJsonData(removeSuperVipRequest));

            return result.IsSuccessStatusCode;
        }

        public async Task<AddRequestResponse> AddSong(AddSongRequest addSongRequest)
        {
            var result = await _playlistClient.PostAsync("AddRequest",
                HttpClientHelper.GetJsonData(addSongRequest));

            return JsonConvert.DeserializeObject<AddRequestResponse>(await result.Content.ReadAsStringAsync());
        }

        public async Task<AddRequestResponse> AddSuperVip(AddSuperVipRequest addSuperVipRequest)
        {
            var result = await _playlistClient.PostAsync("AddSuperRequest",
                HttpClientHelper.GetJsonData(addSuperVipRequest));

            return JsonConvert.DeserializeObject<AddRequestResponse>(await result.Content.ReadAsStringAsync());
        }

        public async Task<EditRequestResponse> EditSuperVip(EditSuperVipRequest editSuperVipRequest)
        {
            var result = await _playlistClient.PostAsync("EditSuperVipRequest",
                HttpClientHelper.GetJsonData(editSuperVipRequest));

            return JsonConvert.DeserializeObject<EditRequestResponse>(await result.Content.ReadAsStringAsync());
        }

        public async Task<int> PromoteSong(PromoteSongRequest promoteSongRequest)
        {
            var result = await _playlistClient.PostAsync("PromoteRequest",
                HttpClientHelper.GetJsonData(promoteSongRequest));

            return JsonConvert.DeserializeObject<int>(await result.Content.ReadAsStringAsync());
        }

        public async Task<bool> ClearRequests()
        {
            var result = await _playlistClient.GetAsync("ClearRequests");

            return result.IsSuccessStatusCode;
        }
    }
}