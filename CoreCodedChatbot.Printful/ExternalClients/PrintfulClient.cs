using System;
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

        public async Task<GetSyncProductsResult> GetAllProducts()
        {
            var result = await _printfulApiClient.GetAsync("store/products");

            if (!result.IsSuccessStatusCode) return null;

            var jsonString = await result.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<GetSyncProductsResult>(jsonString);

            return data;

        }

        public async Task<GetSyncProductsResult> GetRelevantProducts(string searchTerm)
        {
            var result = await _printfulApiClient.GetAsync($"store/products?search={WebUtility.UrlEncode(searchTerm)}");

            if (!result.IsSuccessStatusCode) return null;

            var data = JsonConvert.DeserializeObject<GetSyncProductsResult>(await result.Content.ReadAsStringAsync());

            return data;
        }

        public void Dispose()
        {
            _apiKey = string.Empty;
            _printfulApiClient.Dispose();
        }
    }
}