namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IVipService
    {
        bool GiftVip(string donorUsername, string receiverUsername);
        bool RefundVip(string username, bool deferSave = false);
        bool HasVip(string username);
        bool UseVip(string username);
        bool HasSuperVip(string username);
        bool UseSuperVip(string username);
        bool ModGiveVip(string receivingUsername, int vipsToGive);
    }
}
