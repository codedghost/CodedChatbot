namespace CoreCodedChatbot.Library.Models.Data
{
    public class VipRequests
    {
        public int Donations { get; set; }
        public int Follow { get; set; }
        public int ModGiven { get; set; }
        public int Sub { get; set; }
        public int Byte { get; set; }
        public int Used { get; set; }

        public int TotalRemaining => (Donations + Follow + ModGiven + Sub + Byte) - Used;
    }
}
