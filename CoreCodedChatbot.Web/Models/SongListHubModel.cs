using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Models.Data;

namespace CoreCodedChatbot.Web.Models
{
    public class SongListHubModel
    {
        public string psk { get; set; }
        public PlaylistItem[] requests { get; set; }
    }
}
