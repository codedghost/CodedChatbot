using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Database.Context.Models;

namespace CoreCodedChatbot.Library.Models.Data
{
    public class UserBalance
    {
        private VipRequests _vips { get; set; }
        private string _bytes { get; set; }

        public UserBalance(User user, int bytesConversion)
        {
            _vips = new VipRequests
            {
                Donations = user.DonationOrBitsVipRequests,
                Follow = user.FollowVipRequest,
                ModGiven = user.ModGivenVipRequests,
                Sub = user.SubVipRequests,
                Byte = user.TokenVipRequests,
                Used = user.UsedVipRequests,
                SentGift = user.SentGiftVipRequests,
                ReceivedGift = user.ReceivedGiftVipRequests
            };

            _bytes = (user.TokenBytes / (float) bytesConversion).ToString("n3");
        }

        public int Vips => _vips.TotalRemaining;
        public string Bytes => _bytes;
    }
}
