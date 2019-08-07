using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Username { get; set; }
        public int UsedVipRequests { get; set; }
        public int UsedSuperVipRequests { get; set; }
        public int SentGiftVipRequests { get; set; }
        public int ModGivenVipRequests { get; set; }
        public int FollowVipRequest { get; set; }
        public int SubVipRequests { get; set; }
        public int DonationOrBitsVipRequests { get; set; }
        public int ReceivedGiftVipRequests { get; set; }
        public int TokenVipRequests { get; set; }
        public int TokenBytes { get; set; }
        public int TotalBitsDropped { get; set; }
        public int TotalDonated { get; set; }
        public DateTime TimeLastInChat { get; set; }
    }
}
