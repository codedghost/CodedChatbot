﻿using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.SignalR
{
    public class SongListSingleSongModel
    {
        public string psk { get; set; }
        public PlaylistItem PlaylistItem { get; set; }
    }
}