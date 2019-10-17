using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.StreamStatus;
using CoreCodedChatbot.Library.Models.ApiResponse.StreamStatus;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;

namespace CoreCodedChatbot.ApiClient.ApiClients
{
    public class StreamStatusClient : IStreamStatusClient
    {
        private HttpClient _streamStatusClient;

        public StreamStatusClient(IConfigService configService)
        {
            var config = configService.GetConfig();

            _streamStatusClient = new HttpClient
            {
                BaseAddress = new Uri(config.StreamStatusApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
        }

        public async Task<GetStreamStatusResponse> GetStreamStatus(GetStreamStatusRequest getStreamStatusRequest)
        {
            var result = await _streamStatusClient.GetAsync($"GetStreamStatus?broadcasterUsername={getStreamStatusRequest.BroadcasterUsername}");

            return JsonConvert.DeserializeObject<GetStreamStatusResponse>(await result.Content.ReadAsStringAsync());
        }

        public async Task<bool> SaveStreamStatus(PutStreamStatusRequest putStreamStatusRequest)
        {
            var result = await _streamStatusClient.PutAsync("PuStreamStatus",
                HttpClientHelper.GetJsonData(putStreamStatusRequest));

            return result.IsSuccessStatusCode;
        }
    }
}