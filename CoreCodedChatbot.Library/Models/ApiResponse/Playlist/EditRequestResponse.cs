using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.ApiResponse.Playlist
{
    public class EditRequestResponse
    {
        public string SongRequestText { get; set; }
        public bool SyntaxError { get; set; }
    }
}
