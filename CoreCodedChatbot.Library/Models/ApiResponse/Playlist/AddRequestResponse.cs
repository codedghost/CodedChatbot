using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Library.Models.Enums;

namespace CoreCodedChatbot.Library.Models.ApiResponse.Playlist
{
    public class AddRequestResponse
    {
        public AddRequestResult Result { get; set; }
        public int PlaylistPosition { get; set; }
    }
}
