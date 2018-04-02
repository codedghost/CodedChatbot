using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Models.Data
{
    public class TokenJsonModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}
