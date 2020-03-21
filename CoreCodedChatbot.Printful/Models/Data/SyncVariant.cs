namespace CoreCodedChatbot.Printful.Models.Data
{
    public class SyncVariant
    {
        public int Id { get; set; }
        public string External_Id { get; set; }
        public int Sync_Product_Id { get; set; }
        public string Name { get; set; }
        public bool Synced { get; set; }
        public int Variant_Id { get; set; }
        public string Retail_Price { get; set; }
        public string Currency { get; set; }
        public PrintfulBasicProduct Product { get; set; }
        public PrintfulProductFile[] Files { get; set; }
    }
}