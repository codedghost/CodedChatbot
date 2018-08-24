using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCodedChatbot.Library.Models.ApiResponse.Playlist
{
    public class EditRequestResponse
    {
        public string songRequestText { get; set; }
        public bool syntaxError { get; set; }
    }
}
