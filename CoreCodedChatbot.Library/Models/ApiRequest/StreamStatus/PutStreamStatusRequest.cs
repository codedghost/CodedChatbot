namespace CoreCodedChatbot.Library.Models.ApiRequest.StreamStatus
{
    public class PutStreamStatusRequest
    {
        public string BroadcasterUsername { get; set; }
        public bool IsOnline { get; set; }
    }
}