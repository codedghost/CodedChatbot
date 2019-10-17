using CoreCodedChatbot.Library.Models.ApiRequest.StreamStatus;

namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IStreamStatusService
    {
        bool GetStreamStatus(string broadcasterUsername);
        bool SaveStreamStatus(PutStreamStatusRequest putStreamStatusRequest);
    }
}