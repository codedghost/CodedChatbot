namespace CoreCodedChatbot.Library.Models.Data
{
    public class ChatViewersModel
    {
        public object _links { get; set; }
        public int chatter_count { get; set; }
        public ChattersModel chatters { get; set; }
    }

    public class ChattersModel
    {
        public string[] moderators { get; set; }
        public string[] staff { get; set; }
        public string[] admins { get; set; }
        public string[] global_mods { get; set; }
        public string[] viewers { get; set; }
    }
}
