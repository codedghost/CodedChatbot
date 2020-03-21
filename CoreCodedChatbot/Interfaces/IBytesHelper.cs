using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Interfaces
{
    public interface IBytesHelper
    {
        void GiveViewershipBytes(ChatViewersModel chatViewersModel);
        string CheckBytes(string username);
        bool ConvertByte(string username, int tokensToConvert = 1);
        bool ConvertAllBytes(string username);
        bool GiveGiftSubBytes(string username, int subCount = 1);
    }
}