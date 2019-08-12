using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Helpers;

namespace CoreCodedChatbot.Library.Models.Data
{
    public class VipRequests
    {
        private ConfigModel _config;

        public VipRequests(ConfigModel config)
        {
            _config = config;
        }

        public static VipRequests Create(User user)
        {
            return new VipRequests(ConfigHelper.GetConfig())
            {
                Donations = user.DonationOrBitsVipRequests,
                Follow = user.FollowVipRequest,
                ModGiven = user.ModGivenVipRequests,
                Sub = user.SubVipRequests,
                Byte = user.TokenVipRequests,
                ReceivedGift = user.ReceivedGiftVipRequests,
                Used = user.UsedVipRequests,
                SentGift = user.SentGiftVipRequests,
                UsedSuperVipRequests = user.UsedSuperVipRequests,
                TokenBytes = user.TokenBytes
            };
        }

        public int Donations { get; set; }
        public int Follow { get; set; }
        public int ModGiven { get; set; }
        public int Sub { get; set; }
        public int Byte { get; set; }
        public int Used { get; set; }
        public int SentGift { get; set; }
        public int ReceivedGift { get; set; }
        public int UsedSuperVipRequests { get; set; }

        private int TokenBytes { get; set; }

        public int TotalRemaining => (Donations + Follow + ModGiven + Sub + Byte + ReceivedGift) - (UsedSuperVipRequests * _config.SuperVipCost) - Used - SentGift;
        public string TotalBytes => (TokenBytes / (float) _config.BytesToVip).ToString("n3");
    }
}
