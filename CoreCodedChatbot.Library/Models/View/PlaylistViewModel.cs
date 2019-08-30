using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.View
{
    public class PlaylistViewModel
    {
        public PlaylistItem CurrentSong { get; set; }
        public PlaylistItem[] VipList { get; set; }
        public PlaylistItem[] RegularList { get; set; }
        public LoggedInTwitchUser TwitchUser { get; set; }
    }
}
