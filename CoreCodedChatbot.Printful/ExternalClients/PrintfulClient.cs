using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CoreCodedChatbot.Printful.Interfaces.ExternalClients;
using CoreCodedChatbot.Printful.Models.ApiResponse;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Printful.ExternalClients
{
    public class PrintfulClient : IPrintfulClient, IDisposable
    {
        private string _apiKey;

        private HttpClient _printfulApiClient;
        
        public PrintfulClient(string apiKey, string printfulApiBaseUrl)
        {
            _apiKey = apiKey;

            _printfulApiClient = new HttpClient
            {
                BaseAddress = new Uri(printfulApiBaseUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey)))
                }
            };
        }

        public async Task<List<GetSyncVariantsResult>> GetAllProducts()
        {
            var result = await _printfulApiClient.GetAsync("store/products");

            if (!result.IsSuccessStatusCode) return null;

            var jsonString = await result.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<GetSyncProductsResult>(jsonString);

            return await GetAllVariants(data);
        }

        public async Task<List<GetSyncVariantsResult>> GetRelevantProducts(string searchTerm)
        {
            var result = await _printfulApiClient.GetAsync($"store/products?search={WebUtility.UrlEncode(searchTerm)}");

            if (!result.IsSuccessStatusCode) return null;

            var data = JsonConvert.DeserializeObject<GetSyncProductsResult>(await result.Content.ReadAsStringAsync());

            return await GetAllVariants(data);
        }

        public async Task<GetSyncVariantsResult> GetVariantsById(int id)
        {
            return await GetVariants(id);
        }

        private async Task<GetSyncVariantsResult> GetVariants(int id)
        {
            var result = await _printfulApiClient.GetAsync($"store/products/{id}");
            if (!result.IsSuccessStatusCode) return null;

            var jsonString = await result.Content.ReadAsStringAsync();

            var syncVariantsResult = JsonConvert.DeserializeObject<GetSyncVariantsResult>(jsonString);

            return syncVariantsResult;
        }

        private async Task<List<GetSyncVariantsResult>> GetAllVariants(GetSyncProductsResult getSyncProductsResult)
        {
            List<GetSyncVariantsResult> results = new List<GetSyncVariantsResult>();

            // Now go get the product images and other info
            foreach (var item in getSyncProductsResult.Result)
            {
                var variant = await GetVariants(item.Id);

                if (variant == null) continue;

                results.Add(variant);
            }

            return results;
        }

        public void Dispose()
        {
            _apiKey = string.Empty;
            _printfulApiClient.Dispose();
        }
    }
}