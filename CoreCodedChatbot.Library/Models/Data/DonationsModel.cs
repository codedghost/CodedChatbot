using System;

namespace CoreCodedChatbot.Library.Models.Data
{
    public class DonationsModel
    {
        public StreamLabsDonation[] data { get; set; }
    }

    public class StreamLabsDonation
    {
        public int donation_id { get; set; }
        public string created_at { get; set; }
        public string currency { get; set; }
        public double amount { get; set; }
        public string name { get; set; }
        public string message { get; set; }
        public string email { get; set; }
    }
}
