namespace CoreCodedChatbot.Library.Models.ApiRequest.Playlist
{
    public class AddSongRequest
    {
        public string username { get; set; }
        public string commandText { get; set; }
        public bool isVipRequest = false;
    }
}
