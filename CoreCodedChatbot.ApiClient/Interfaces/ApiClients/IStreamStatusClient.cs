using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.ApiRequest.StreamStatus;
using CoreCodedChatbot.Library.Models.ApiResponse.StreamStatus;

namespace CoreCodedChatbot.ApiClient.Interfaces.ApiClients
{
    public interface IStreamStatusClient
    {
        Task<GetStreamStatusResponse> GetStreamStatus(GetStreamStatusRequest getStreamStatusRequest);
        Task<bool> SaveStreamStatus(PutStreamStatusRequest putStreamStatusRequest);
    }
}