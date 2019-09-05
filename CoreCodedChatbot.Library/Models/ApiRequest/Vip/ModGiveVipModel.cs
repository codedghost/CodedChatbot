using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.ApiRequest.Vip
{
    public class ModGiveVipModel
    {
        public string ReceivingUsername { get; set; }
        public int VipsToGive { get; set; }
    }
}
