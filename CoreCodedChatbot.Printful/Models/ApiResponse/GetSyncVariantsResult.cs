using CoreCodedChatbot.Printful.Models.Data;

namespace CoreCodedChatbot.Printful.Models.ApiResponse
{
    public class GetSyncVariantsResult
    {
        public int Code { get; set; }
        public VariantQueryResult Result { get; set; }
    }
}