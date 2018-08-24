using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Database.Context.Models;

namespace CoreCodedChatbot.Library.Models.Data
{
    public class PlaylistItem
    {
        public int songRequestId { get; set; }
        public string songRequestText { get; set; }
        public bool isInChat { get; set; }
    }
}
