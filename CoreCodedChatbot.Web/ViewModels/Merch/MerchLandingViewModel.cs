using CoreCodedChatbot.Printful.Models.Data;

namespace CoreCodedChatbot.Web.ViewModels.Merch
{
    public class MerchLandingViewModel
    {
        public string SearchTerms { get; set; }
        public SyncProduct[] SyncProducts { get; set; }
    }
}