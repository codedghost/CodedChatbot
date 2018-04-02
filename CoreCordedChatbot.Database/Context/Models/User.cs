using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Text;

namespace CoreCodedChatbot.Database.Context.Models
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Username { get; set; }
        public int UsedVipRequests { get; set; }
        public int ModGivenVipRequests { get; set; }
        public int FollowVipRequest { get; set; }
        public int SubVipRequests { get; set; }
        public int DonationOrBitsVipRequests { get; set; }
        public int TokenVipRequests { get; set; }
        public int TokenBytes { get; set; }
    }
}
