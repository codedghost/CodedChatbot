using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.View
{
    public class PlaylistBrowserSource
    {
        public PlaylistItem[] Songs { get; set; }
        public LoggedInTwitchUser TwitchUser { get; set; }
    }
}
