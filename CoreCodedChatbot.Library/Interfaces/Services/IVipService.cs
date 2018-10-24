using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IVipService
    {
        bool GiftVip(string donorUsername, string receiverUsername);
        bool RefundVip(string username);
    }
}
