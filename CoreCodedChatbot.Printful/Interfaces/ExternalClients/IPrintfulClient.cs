
using System.Threading.Tasks;
using CoreCodedChatbot.Printful.Models.ApiResponse;

namespace CoreCodedChatbot.Printful.Interfaces.ExternalClients
{
    public interface IPrintfulClient
    {
        Task<GetSyncProductsResult> GetAllProducts();
        Task<GetSyncProductsResult> GetRelevantProducts(string searchTerm);
    }
}