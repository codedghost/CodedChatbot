using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.ApiClient.ApiClients
{
    public class PlaylistApiClient : IPlaylistService
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
    }
}