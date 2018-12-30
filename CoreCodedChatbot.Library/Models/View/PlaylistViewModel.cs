using CoreCodedChatbot.Library.Models.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CoreCodedChatbot.Library.Models.View
{
    public class PlaylistViewModel
    {
        public PlaylistItem CurrentSong { get; set; }
        public PlaylistItem[] VipList { get; set; }
        public PlaylistItem[] RegularList { get; set; }
        public SelectListItem[] RequestInstruments { get; set; }
        public LoggedInTwitchUser TwitchUser { get; set; }
    }
}
