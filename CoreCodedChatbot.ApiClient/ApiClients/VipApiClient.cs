using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.ApiClients;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.Vip;

namespace CoreCodedChatbot.Library.ApiClients
{
    public class VipApiClient : IVipApiClient
    {
        private HttpClient _client;

        public VipApiClient(IConfigService configService)
        {
            var config = configService.GetConfig();

            _client = new HttpClient
            {
                BaseAddress = new Uri(config.VipApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };
        }


        public async Task<bool> GiftVip(GiftVipModel giftVipModel)
        {
            var result =  await _client.PostAsync("GiftVip",
                   HttpClientHelper.GetJsonData(giftVipModel));

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> ModGiveVip(ModGiveVipModel modGiveVipModel)
        {
            var result = await _client.PostAsync("ModGiveVip", HttpClientHelper.GetJsonData(modGiveVipModel));

            return result.IsSuccessStatusCode;
        }
    }
}