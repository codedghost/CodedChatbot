using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Web.Interfaces
{
    public interface IChatterService
    {
        void UpdateChatters();
        ChatViewersModel GetCurrentChatters();
    }
}
