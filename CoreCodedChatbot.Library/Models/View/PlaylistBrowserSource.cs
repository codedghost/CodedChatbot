using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.View
{
    public class PlaylistBrowserSource
    {
        public PlaylistItem[] VipList { get; set; }
        public PlaylistItem[] RegularList { get; set; }
        public LoggedInTwitchUser TwitchUser { get; set; }
    }
}
