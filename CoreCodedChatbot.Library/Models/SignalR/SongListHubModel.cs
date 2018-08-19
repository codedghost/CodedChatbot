using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.SignalR
{
    public class SongListHubModel
    {
        public string psk { get; set; }
        public PlaylistItem[] requests { get; set; }
    }
}
