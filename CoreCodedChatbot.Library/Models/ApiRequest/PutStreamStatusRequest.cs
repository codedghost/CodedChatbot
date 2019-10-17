namespace CoreCodedChatbot.Library.Models.ApiRequest
{
    public class PutStreamStatusRequest
    {
        public string BroadcasterUsername { get; set; }
        public bool IsOnline { get; set; }
    }
}