namespace CoreCodedChatbot.Printful.Models.Data
{
    public class SyncProduct
    {
        public int Id { get; set; }
        public string External_Id { get; set; }
        public string Name { get; set; }
        public int Variants { get; set; }
        public int Synced { get; set; }
    }
}