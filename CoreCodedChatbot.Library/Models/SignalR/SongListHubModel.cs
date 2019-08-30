using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.SignalR
{
    public class SongListHubModel
    {
        public string psk { get; set; }
        public PlaylistItem currentSong { get; set; }
        public PlaylistItem[] regularRequests { get; set; }
        public PlaylistItem[] vipRequests { get; set; }
    }
}
