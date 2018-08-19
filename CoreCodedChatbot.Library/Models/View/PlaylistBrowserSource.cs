using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.View
{
    public class PlaylistBrowserSource
    {
        public PlaylistItem[] List { get; set; }
        public LoggedInTwitchUser TwitchUser { get; set; }
    }
}
