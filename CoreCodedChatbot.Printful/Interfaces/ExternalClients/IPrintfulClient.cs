
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreCodedChatbot.Printful.Models.ApiResponse;

namespace CoreCodedChatbot.Printful.Interfaces.ExternalClients
{
    public interface IPrintfulClient
    {
        Task<List<GetSyncVariantsResult>> GetAllProducts();
        Task<List<GetSyncVariantsResult>> GetRelevantProducts(string searchTerm);
    }
}