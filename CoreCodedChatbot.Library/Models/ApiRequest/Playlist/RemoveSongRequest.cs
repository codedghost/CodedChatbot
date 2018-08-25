using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.ApiRequest.Playlist
{
    public class RemoveSongRequest
    {
        public string username { get; set; }
        public string commandText { get; set; }
        public bool isMod { get; set; }
    }
}
