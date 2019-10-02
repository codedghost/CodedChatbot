using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Printful.Models.Data;

namespace CoreCodedChatbot.Printful.Models.ApiResponse
{
    public class GetSyncProductsResult
    {
        public int Code { get; set; }
        public SyncProduct[] Result { get; set; }
        public ApiPaging Paging { get; set; }
    }
}
