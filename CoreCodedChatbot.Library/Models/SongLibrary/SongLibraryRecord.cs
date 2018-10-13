using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Library.Models.SongLibrary
{
    public class SongLibraryRecord
    {
        public int rowId { get; set; }
        public string colTitle { get; set; }
        public string colArtist { get; set; }
    }
}
