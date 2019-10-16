using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.ApiRequest.Vip;

namespace CoreCodedChatbot.Library.Interfaces.ApiClients
{
    public interface IVipApiClient
    {
        Task<bool> GiftVip(GiftVipModel giftVipModel);
        Task<bool> ModGiveVip(ModGiveVipModel modGiveVipModel);
    }
}