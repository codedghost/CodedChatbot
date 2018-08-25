using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.ApiRequest.Playlist
{
    public class PromoteSongRequest
    {
        public string username { get; set; }
        public int songIndex { get; set; }
    }
}
