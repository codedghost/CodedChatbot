using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Models.Data
{
    public class DonationsModel
    {
        public StreamLabsDonation[] StreamLabsDonations { get; set; }
    }

    public class StreamLabsDonation
    {
        public int donation_id { get; set; }
        public DateTime created_at { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string name { get; set; }
        public string message { get; set; }
        public string email { get; set; }
    }
}
