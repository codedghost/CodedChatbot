namespace CoreCodedChatbot.Library.Models.ApiRequest.Playlist
{
    public class EditSongRequest
    {
        public string username { get; set; }
        public string commandText { get; set; }
        public bool isMod { get; set; }
    }
}
